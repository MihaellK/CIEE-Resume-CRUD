import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import axios from 'axios';
import ResumeForm from './ResumeForm';

// Dizemos ao Vitest para interceptar todas as chamadas do módulo axios
vi.mock('axios');

describe('ResumeForm Component', () => {
  it('Should display validation error when name is empty and form is submitted', async () => {
    // Arrange
    render(<ResumeForm />);
    
    // Act
    const submitButton = screen.getByRole('button', { name: /enviar/i });
    fireEvent.click(submitButton);

    // Assert
    await waitFor(() => {
      expect(screen.getByText(/O nome do candidato é obrigatório/i)).toBeInTheDocument();
    });
  });

  // Fase Red para o Arquivo PDF
  it('Should display validation error when PDF file is not selected', async () => {
    // Arrange
    render(<ResumeForm />);
    
    // Preenchemos o nome para isolar o erro do ficheiro
    const nameInput = screen.getByLabelText(/nome do candidato/i);
    fireEvent.change(nameInput, { target: { value: 'Mihaell Alves' } });
    
    // Act - Tentamos submeter sem selecionar o ficheiro PDF
    const submitButton = screen.getByRole('button', { name: /enviar/i });
    fireEvent.click(submitButton);

    // Assert
    await waitFor(() => {
      expect(screen.getByText(/O currículo em PDF é obrigatório/i)).toBeInTheDocument();
    });
  });

  it('Should display validation error when file is not a PDF', async () => {
    render(<ResumeForm />);
    
    // Preenchemos o nome validamente
    const nameInput = screen.getByLabelText(/nome do candidato/i);
    fireEvent.change(nameInput, { target: { value: 'Mihaell Alves' } });
    
    // Act - Simulamos o upload de um ficheiro com extensão e MIME type incorretos (exemplo imagem ou txt)
    const fileInput = screen.getByLabelText(/currículo \(pdf\)/i);
    const invalidFile = new File(['conteudo de texto'], 'testoInvalido.txt', { type: 'text/plain' });
    fireEvent.change(fileInput, { target: { files: [invalidFile] } });

    const submitButton = screen.getByRole('button', { name: /enviar/i });
    fireEvent.click(submitButton);

    // Assert
    await waitFor(() => {
      expect(screen.getByText(/O ficheiro deve ser um PDF/i)).toBeInTheDocument();
    });
  });

  it('Should display validation error when file exceeds 5MB', async () => {
    render(<ResumeForm />);
    
    const nameInput = screen.getByLabelText(/nome do candidato/i);
    fireEvent.change(nameInput, { target: { value: 'Mihaell Alves' } });
    
    // Act - Simulamos um ficheiro PDF pesado (6MB) manipulando a propriedade size do objeto File
    const fileInput = screen.getByLabelText(/currículo \(pdf\)/i);
    const largeFile = new File(['conteudo dummy'], 'curriculo_pesado.pdf', { type: 'application/pdf' });
    Object.defineProperty(largeFile, 'size', { value: 6 * 1024 * 1024 }); 
    
    fireEvent.change(fileInput, { target: { files: [largeFile] } });

    const submitButton = screen.getByRole('button', { name: /enviar/i });
    fireEvent.click(submitButton);

    // Assert
    await waitFor(() => {
      expect(screen.getByText(/O ficheiro não pode exceder 5MB/i)).toBeInTheDocument();
    });
  });

  // Fase Red para a integração com a API
  it('Should call the API with FormData when valid data is submitted', async () => {
    // Arrange
    // Tipamos o mock para o TypeScript reconhecer os métodos do Jest/Vitest
    const mockedAxios = vi.mocked(axios);
    mockedAxios.post.mockResolvedValueOnce({ data: { id: '123', name: 'Mihaell Alves' } });

    render(<ResumeForm />);
    
    const nameInput = screen.getByLabelText(/nome do candidato/i);
    fireEvent.change(nameInput, { target: { value: 'Mihaell Alves' } });
    
    const fileInput = screen.getByLabelText(/currículo \(pdf\)/i);
    const validFile = new File(['dummy content'], 'curriculo.pdf', { type: 'application/pdf' });
    fireEvent.change(fileInput, { target: { files: [validFile] } });

    // Act
    const submitButton = screen.getByRole('button', { name: /enviar/i });
    fireEvent.click(submitButton);

    // Assert
    await waitFor(() => {
      // Verifica se o axios.post foi chamado exatamente 1 vez
      expect(mockedAxios.post).toHaveBeenCalledTimes(1);
      
      // Verifica se a chamada foi feita para o endpoint correto e com FormData
      expect(mockedAxios.post).toHaveBeenCalledWith(
        'https://localhost:1433/api/resumes/upload', // O URL real será configurado depois via env vars
        expect.any(FormData),
        expect.objectContaining({
          headers: { 'Content-Type': 'multipart/form-data' }
        })
      );
    });
  });

});