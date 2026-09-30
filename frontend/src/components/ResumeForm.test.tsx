import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import ResumeForm from './ResumeForm';

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

});