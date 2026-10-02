import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import axios from 'axios';
import ResumeForm from './ResumeForm';

vi.mock('axios');

describe('ResumeForm Component - Novos Requisitos', () => {
  const mockedAxios = vi.mocked(axios);

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('Deve exibir erro de validação se Nome e Email estiverem vazios na submissão manual', async () => {
    render(<ResumeForm />);
    
    const submitButton = screen.getByRole('button', { name: /salvar currículo/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/O nome.*obrigatório/i)).toBeInTheDocument();
      expect(screen.getByText(/O e-mail.*obrigatório/i)).toBeInTheDocument();
    });
  });

  it('Deve enviar os dados corretamente (sem PDF) para a API principal (Cadastro Manual)', async () => {
    mockedAxios.post.mockResolvedValueOnce({ data: { id: '123' } });
    render(<ResumeForm />);
    
    fireEvent.change(screen.getByLabelText(/nome completo/i), { target: { value: 'Mihaell Alves' } });
    fireEvent.change(screen.getByLabelText(/e-mail/i), { target: { value: 'mihaell@teste.com' } });
    fireEvent.change(screen.getByLabelText(/área ou cargo/i), { target: { value: 'Engenharia' } });
    
    fireEvent.click(screen.getByRole('button', { name: /salvar currículo/i }));

    await waitFor(() => {
      expect(mockedAxios.post).toHaveBeenCalledTimes(1);
      // Agora esperamos que o submit envie JSON e não FormData, pois o PDF não vai para o BD
      expect(mockedAxios.post).toHaveBeenCalledWith(
        expect.stringContaining('/api/resumes'),
        expect.objectContaining({
          name: 'Mihaell Alves',
          email: 'mihaell@teste.com',
          areaOfInterest: 'Engenharia'
        }),
        expect.objectContaining({
          headers: { 'Content-Type': 'application/json' }
        })
      );
    });
  });

  it('Deve chamar o endpoint /parse e preencher os inputs ao fazer upload de um PDF (Autofill)', async () => {
    // Simulamos a resposta do backend de extração
    mockedAxios.post.mockResolvedValueOnce({ 
      data: { name: 'João Extraído', email: 'joao@extraido.com', phone: '11999999999' } 
    });

    render(<ResumeForm />);
    
    const fileInput = screen.getByLabelText(/fazer upload de currículo \(pdf autofill\)/i);
    const validFile = new File(['dummy'], 'curriculo.pdf', { type: 'application/pdf' });
    
    // Ao inserir o ficheiro, o evento deve disparar a chamada à API de parse imediatamente
    fireEvent.change(fileInput, { target: { files: [validFile] } });

    await waitFor(() => {
      expect(mockedAxios.post).toHaveBeenCalledWith(
        expect.stringContaining('/api/resumes/parse'),
        expect.any(FormData),
        expect.objectContaining({ headers: { 'Content-Type': 'multipart/form-data' } })
      );
    });

    // Verifica se os inputs foram preenchidos (Autofill)
    expect(screen.getByLabelText(/nome completo/i)).toHaveValue('João Extraído');
    expect(screen.getByLabelText(/e-mail/i)).toHaveValue('joao@extraido.com');
  });
});