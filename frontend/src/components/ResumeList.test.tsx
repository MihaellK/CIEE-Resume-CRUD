import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import axios from 'axios';
import { MemoryRouter } from 'react-router-dom';
import ResumeList from './ResumeList';

// Intercetamos o axios para não fazer chamadas reais de rede durante o teste unitário
vi.mock('axios');

describe('ResumeList Component', () => {
  const mockedAxios = vi.mocked(axios);

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('Should fetch and display a list of resumes with details link', async () => {
    // Simulamos a resposta da nossa API
    const mockData = [
      { id: '1', name: 'João Silva', email: 'joao@email.com', phone: '999999999' },
      { id: '2', name: 'Maria Santos', email: 'maria@email.com', phone: '888888888' }
    ];
    
    mockedAxios.get.mockResolvedValueOnce({ data: mockData });

    // Renderizamos envolto num MemoryRouter porque agora temos <Link />
    render(
      <MemoryRouter>
        <ResumeList />
      </MemoryRouter>
    );

    // Assert - Valida estado de loading inicial (opcional mas recomendado para boa UX)
    expect(screen.getByText(/A carregar.../i)).toBeInTheDocument();

    // Assert - Valida a renderização dos dados após a resolução da Promise
    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
      
      // Valida se a coluna de Ações e os links foram renderizados corretamente
      const detailsLinks = screen.getAllByRole('link', { name: /ver detalhes/i });
      expect(detailsLinks).toHaveLength(2);
      expect(detailsLinks[0]).toHaveAttribute('href', '/resumes/1');
    });
  });

  it('Should display an error message if the API call fails', async () => {
    mockedAxios.get.mockRejectedValueOnce(new Error('Network Error'));

    render(
      <MemoryRouter>
        <ResumeList />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Erro ao carregar os currículos/i)).toBeInTheDocument();
    });
  });
});