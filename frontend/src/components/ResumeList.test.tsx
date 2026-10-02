import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import axios from 'axios';
import ResumeList from './ResumeList';

// Intercetamos o axios para não fazer chamadas reais de rede durante o teste unitário
vi.mock('axios');

describe('ResumeList Component', () => {
  it('Should fetch and display a list of resumes', async () => {
    // Arrange
    const mockedAxios = vi.mocked(axios);
    
    // Simulamos a resposta da nossa API
    const mockData = [
      { id: '1', name: 'João Silva', email: 'joao@email.com', phone: '999999999' },
      { id: '2', name: 'Maria Santos', email: 'maria@email.com', phone: '888888888' }
    ];
    
    mockedAxios.get.mockResolvedValueOnce({ data: mockData });

    // Act
    render(<ResumeList />);

    // Assert - Valida estado de loading inicial (opcional mas recomendado para boa UX)
    expect(screen.getByText(/A carregar.../i)).toBeInTheDocument();

    // Assert - Valida a renderização dos dados após a resolução da Promise
    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
      expect(screen.getByText('maria@email.com')).toBeInTheDocument();
      expect(screen.getByText('999999999')).toBeInTheDocument();
      
      // O URL base que o componente realmente calcula
      const expectedUrl = import.meta.env.VITE_API_URL 
        ? import.meta.env.VITE_API_URL.replace('/upload', '') 
        : 'http://localhost:5092/api/resumes';

      // Garante que o axios.get foi chamado exatamente 1 vez
      expect(mockedAxios.get).toHaveBeenCalledTimes(1);
      
      // Valida o URL correto e a presença do AbortSignal na configuração
      expect(mockedAxios.get).toHaveBeenCalledWith(
        expectedUrl,
        expect.objectContaining({
          signal: expect.any(AbortSignal)
        })
      );
    });
  });

  it('Should display an error message if the API call fails', async () => {
    // Arrange
    const mockedAxios = vi.mocked(axios);
    mockedAxios.get.mockRejectedValueOnce(new Error('Network Error'));

    // Act
    render(<ResumeList />);

    // Assert
    await waitFor(() => {
      expect(screen.getByText(/Erro ao carregar os currículos/i)).toBeInTheDocument();
    });
  });
});