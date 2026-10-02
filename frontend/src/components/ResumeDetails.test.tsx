import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import axios from 'axios';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import ResumeDetails from './ResumeDetails';

vi.mock('axios');

describe('ResumeDetails Component', () => {
  const mockedAxios = vi.mocked(axios);

  beforeEach(() => {
    vi.clearAllMocks();
  });

  // Função auxiliar para injetar o Router no teste com um ID fictício na URL
  const renderWithRouter = (id: string) => {
    return render(
      <MemoryRouter initialEntries={[`/resumes/${id}`]}>
        <Routes>
          <Route path="/resumes/:id" element={<ResumeDetails />} />
        </Routes>
      </MemoryRouter>
    );
  };

  it('Deve exibir mensagem de carregamento inicialmente', () => {
    // Retorna uma Promise pendente para travar o estado em "Loading"
    mockedAxios.get.mockImplementation(() => new Promise(() => {})); 
    
    renderWithRouter('123');
    
    expect(screen.getByText(/carregando/i)).toBeInTheDocument();
  });

  it('Deve renderizar os dados do currículo quando a API retorna 200 OK', async () => {
    const mockData = {
      id: '123',
      name: 'Mihaell Alves',
      email: 'mihaell@teste.com',
      phone: '11999999999',
      areaOfInterest: 'Engenharia de Software',
      professionalSummary: 'Especialista em React e .NET'
    };

    mockedAxios.get.mockResolvedValueOnce({ data: mockData });

    renderWithRouter('123');

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Mihaell Alves' })).toBeInTheDocument();
      expect(screen.getByText('mihaell@teste.com')).toBeInTheDocument();
      expect(screen.getByText('Engenharia de Software')).toBeInTheDocument();
      expect(screen.getByText('Especialista em React e .NET')).toBeInTheDocument();
    });
  });

    it('Deve exibir mensagem de erro quando o currículo não é encontrado (404)', async () => {
        // Simula a falha da API com status 404 e diz ao Vitest que É um AxiosError
        mockedAxios.isAxiosError.mockReturnValueOnce(true);
        mockedAxios.get.mockRejectedValueOnce({ response: { status: 404 } });

        renderWithRouter('999');

        await waitFor(() => {
        expect(screen.getByText(/currículo não encontrado/i)).toBeInTheDocument();
        });
    });
});