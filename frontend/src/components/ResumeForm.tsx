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

  return (
    <div style={{ marginBottom: '2rem' }}>
      <h2>Novo Currículo</h2>
      
      {globalError && <div style={{ color: 'red', marginBottom: '1rem' }}>{globalError}</div>}
      {successMessage && <div style={{ color: 'green', marginBottom: '1rem' }}>{successMessage}</div>}

      <div style={{ marginBottom: '1.5rem', padding: '1rem', backgroundColor: '#f9f9f9', border: '1px dashed #ccc' }}>
        <label htmlFor="pdf-upload" style={{ display: 'block', fontWeight: 'bold' }}>
          Fazer upload de currículo (PDF Autofill)
        </label>
        <p style={{ fontSize: '0.85rem', color: '#666' }}>Opcional. Envie um PDF para preencher os dados automaticamente.</p>
        <input 
          id="pdf-upload" 
          type="file" 
          accept=".pdf" 
          onChange={handlePdfUpload} 
          disabled={isParsing || isSubmitting}
        />
        {isParsing && <span style={{ marginLeft: '1rem', color: '#0066cc' }}>A extrair dados...</span>}
      </div>

      <form onSubmit={handleSubmit(onSubmit)} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <div>
          <label htmlFor="name" style={{ display: 'block' }}>Nome Completo *</label>
          <input id="name" style={{ width: '100%', padding: '0.5rem' }} {...register('name')} disabled={isSubmitting} />
          {errors.name && <span style={{ color: 'red', fontSize: '0.85rem' }}>{errors.name.message}</span>}
        </div>

        <div>
          <label htmlFor="email" style={{ display: 'block' }}>E-mail *</label>
          <input id="email" type="email" style={{ width: '100%', padding: '0.5rem' }} {...register('email')} disabled={isSubmitting} />
          {errors.email && <span style={{ color: 'red', fontSize: '0.85rem' }}>{errors.email.message}</span>}
        </div>

        <div>
          <label htmlFor="phone" style={{ display: 'block' }}>Telefone</label>
          <input id="phone" style={{ width: '100%', padding: '0.5rem' }} {...register('phone')} disabled={isSubmitting} />
        </div>

        <div>
          <label htmlFor="areaOfInterest" style={{ display: 'block' }}>Área ou Cargo de Interesse</label>
          <input id="areaOfInterest" style={{ width: '100%', padding: '0.5rem' }} {...register('areaOfInterest')} disabled={isSubmitting} />
        </div>

        <div>
          <label htmlFor="professionalSummary" style={{ display: 'block' }}>Resumo Profissional</label>
          <textarea id="professionalSummary" rows={4} style={{ width: '100%', padding: '0.5rem' }} {...register('professionalSummary')} disabled={isSubmitting} />
        </div>

        <button type="submit" disabled={isParsing || isSubmitting} style={{ padding: '0.75rem', marginTop: '1rem', cursor: 'pointer' }}>
          {isSubmitting ? 'A salvar...' : 'Salvar Currículo'}
        </button>
      </form>
    </div>
  );
};

export default ResumeForm;