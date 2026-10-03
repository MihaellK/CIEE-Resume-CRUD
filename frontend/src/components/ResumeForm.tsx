import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import axios from 'axios';

const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ACCEPTED_FILE_TYPES = ['application/pdf'];

// 1. Definição do Schema de Validação com Zod
const resumeSchema = z.object({
  name: z.string().min(1, 'O nome do candidato é obrigatório.'),
  // Colocamos o min(1) antes para garantir que campos vazios exibem a mensagem de obrigatoriedade
  email: z.string().min(1, 'O e-mail do candidato é obrigatório.').email('O e-mail fornecido é inválido.'),
  phone: z.string().optional(),
  areaOfInterest: z.string().optional(),
  professionalSummary: z.string().optional(),
});

type ResumeFormData = z.infer<typeof resumeSchema>;

const ResumeForm: React.FC = () => {
  const [isParsing, setIsParsing] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [globalError, setGlobalError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<ResumeFormData>({
    resolver: zodResolver(resumeSchema),
  });

  // 2. Lógica de Autofill via Endpoint /parse
  const handlePdfUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validações de UI utilizando o método .includes() para validar contra o array
    if (!ACCEPTED_FILE_TYPES.includes(file.type)) {
      setGlobalError('O ficheiro deve ser um PDF válido.');
      return;
    }
    if (file.size > MAX_FILE_SIZE) {
      setGlobalError('O ficheiro não pode exceder 5MB.');
      return;
    }

    setIsParsing(true);
    setGlobalError(null);

    const formData = new FormData();
    formData.append('file', file);

    try {
      const baseUrl = import.meta.env.VITE_API_URL 
        ? import.meta.env.VITE_API_URL.replace('/upload', '') 
        : 'http://localhost:5092/api/resumes';
        
      const response = await axios.post(`${baseUrl}/parse`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });

      const { name, email, phone } = response.data;
      
      if (name) setValue('name', name);
      if (email) setValue('email', email);
      if (phone) setValue('phone', phone);
      
    } catch (err) {
      console.error(err);
      setGlobalError('Falha ao extrair dados do PDF. Pode continuar a preencher manualmente.');
    } finally {
      setIsParsing(false);
      e.target.value = ''; 
    }
  };

  // 3. Lógica de Submissão do Formulário (Cadastro Manual)
  const onSubmit = async (data: ResumeFormData) => {
    setIsSubmitting(true);
    setGlobalError(null);
    setSuccessMessage(null);

    try {
      const baseUrl = import.meta.env.VITE_API_URL 
        ? import.meta.env.VITE_API_URL.replace('/upload', '') 
        : 'http://localhost:5092/api/resumes';
        
      await axios.post(baseUrl, data, {
        // Corrigido: Aqui enviamos JSON para a API criar o registo
        headers: { 'Content-Type': 'application/json' }
      });
      
      setSuccessMessage('Currículo salvo com sucesso!');
    } catch (err) {
      console.error(err);
      setGlobalError('Erro ao salvar o currículo. Verifique os dados e tente novamente.');
    } finally {
      setIsSubmitting(false);
    }
  };

  // Estilos base reutilizáveis
  const inputStyle = {
    width: '100%',
    padding: '0.75rem',
    marginTop: '0.25rem',
    border: '1px solid #ccc',
    borderRadius: '4px',
    fontSize: '1rem',
    boxSizing: 'border-box' as const,
  };

  const labelStyle = {
    display: 'block',
    fontWeight: '600',
    color: '#333',
    marginTop: '1rem',
    fontSize: '0.9rem'
  };

  const errorStyle = {
    color: '#d32f2f',
    fontSize: '0.85rem',
    marginTop: '0.25rem',
    display: 'block'
  };

  return (
    <div style={{
      maxWidth: '600px',
      margin: '0 auto',
      padding: '2rem',
      backgroundColor: '#ffffff',
      borderRadius: '8px',
      boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)',
      fontFamily: 'system-ui, -apple-system, sans-serif'
    }}>
      <h2 style={{ marginTop: 0, color: '#1a1a1a', borderBottom: '2px solid #f0f0f0', paddingBottom: '0.5rem' }}>
        Novo Currículo
      </h2>

      {globalError && (
        <div role="alert" style={{ padding: '1rem', backgroundColor: '#ffebee', color: '#c62828', borderRadius: '4px', marginBottom: '1rem' }}>
          {globalError}
        </div>
      )}

      {successMessage && (
        <div role="alert" style={{ padding: '1rem', backgroundColor: '#e8f5e9', color: '#2e7d32', borderRadius: '4px', marginBottom: '1rem' }}>
          Currículo cadastrado com sucesso!
        </div>
      )}

      <div style={{ backgroundColor: '#f8f9fa', padding: '1.5rem', borderRadius: '6px', marginBottom: '1.5rem', border: '1px dashed #ced4da' }}>
        <h3 style={{ marginTop: 0, fontSize: '1.1rem', color: '#495057' }}>Preenchimento Automático (Autofill)</h3>
        <p style={{ fontSize: '0.9rem', color: '#6c757d', marginBottom: '1rem' }}>
          Opcional. Faça upload de um currículo em PDF para extrair os dados básicos.
        </p>
        <input 
          type="file" 
          accept="application/pdf" 
          onChange={handlePdfUpload}
          disabled={isParsing || isSubmitting}
          aria-label="Upload de PDF para preenchimento automático"
        />
        {isParsing && <span aria-live="polite" style={{ marginLeft: '1rem', fontSize: '0.9rem', color: '#0066cc' }}>Processando documento...</span>}
      </div>

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <div>
          <label htmlFor="name" style={labelStyle}>Nome Completo *</label>
          <input
            id="name"
            type="text"
            placeholder="Ex: João da Silva"
            style={{ ...inputStyle, borderColor: errors.name ? '#d32f2f' : '#ccc' }}
            aria-invalid={!!errors.name}
            {...register('name')}
          />
          {errors.name && <span style={errorStyle} role="alert">{errors.name.message}</span>}
        </div>

        <div>
          <label htmlFor="email" style={labelStyle}>E-mail *</label>
          <input
            id="email"
            type="email"
            placeholder="Ex: joao@email.com"
            style={{ ...inputStyle, borderColor: errors.email ? '#d32f2f' : '#ccc' }}
            aria-invalid={!!errors.email}
            {...register('email')}
          />
          {errors.email && <span style={errorStyle} role="alert">{errors.email.message}</span>}
        </div>

        <div>
          <label htmlFor="phone" style={labelStyle}>Telefone</label>
          <input
            id="phone"
            type="tel"
            placeholder="Ex: (11) 99999-9999"
            style={inputStyle}
            {...register('phone')}
          />
        </div>

        <div>
          <label htmlFor="areaOfInterest" style={labelStyle}>Área ou Cargo de Interesse</label>
          <input
            id="areaOfInterest"
            type="text"
            placeholder="Ex: Engenharia de Software"
            style={inputStyle}
            {...register('areaOfInterest')}
          />
        </div>

        <div>
          <label htmlFor="professionalSummary" style={labelStyle}>Resumo Profissional</label>
          <textarea
            id="professionalSummary"
            rows={4}
            placeholder="Descreva brevemente as qualificações..."
            style={{ ...inputStyle, resize: 'vertical' }}
            {...register('professionalSummary')}
          />
        </div>

        <button 
          type="submit" 
          disabled={isSubmitting || isParsing}
          style={{
            width: '100%',
            padding: '1rem',
            marginTop: '2rem',
            backgroundColor: (isSubmitting || isParsing) ? '#9ca3af' : '#2563eb',
            color: '#ffffff',
            border: 'none',
            borderRadius: '4px',
            fontSize: '1rem',
            fontWeight: '600',
            cursor: (isSubmitting || isParsing) ? 'not-allowed' : 'pointer',
            transition: 'background-color 0.2s'
          }}
        >
          {isSubmitting ? 'A Salvar...' : 'Salvar Currículo'}
        </button>
      </form>
    </div>
  );
};

export default ResumeForm;