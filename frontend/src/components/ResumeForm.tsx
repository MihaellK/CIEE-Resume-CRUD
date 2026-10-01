import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import axios from 'axios';

const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ACCEPTED_FILE_TYPES = ['application/pdf'];

const schema = z.object({
  name: z.string().min(1, 'O nome do candidato é obrigatório'),
  file: z.any()
    .refine((files) => files && files.length > 0, 'O currículo em PDF é obrigatório')
    .refine((files) => {
      if (!files || files.length === 0) return true;
      return ACCEPTED_FILE_TYPES.includes(files[0].type);
    }, 'O ficheiro deve ser um PDF')
    .refine((files) => {
      if (!files || files.length === 0) return true;
      return files[0].size <= MAX_FILE_SIZE;
    }, 'O ficheiro não pode exceder 5MB')
});

type ResumeFormData = z.infer<typeof schema>;

const ResumeForm: React.FC = () => {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [feedback, setFeedback] = useState<{ message: string, type: 'success' | 'error' } | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ResumeFormData>({
    resolver: zodResolver(schema),
    mode: 'onSubmit'
  });

  const onSubmit = async (data: ResumeFormData) => {
    setIsSubmitting(true);
    setFeedback(null);

    // Constrói o payload adequado para upload de ficheiros
    const formData = new FormData();
    formData.append('name', data.name);
    formData.append('file', data.file[0]);

    try {
      // URL chumbado por agora para passar no teste. Será movido para .env depois.
      await axios.post(import.meta.env.VITE_API_URL, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      
      setFeedback({ message: 'Currículo enviado com sucesso!', type: 'success' });
      reset(); // Limpa o formulário após sucesso
    } catch (error) {
      setFeedback({ message: 'Ocorreu um erro ao enviar o currículo. Tente novamente.', type: 'error' });
      console.error(error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      {feedback && (
        <div style={{ color: feedback.type === 'success' ? 'green' : 'red', marginBottom: '15px' }}>
          <strong>{feedback.message}</strong>
        </div>
      )}

      <div>
        <label htmlFor="name">Nome do Candidato</label>
        <input 
          id="name" 
          {...register('name')} 
          placeholder="Digite o seu nome"
          disabled={isSubmitting}
        />
        {errors.name && <span style={{ color: 'red', display: 'block' }}>{errors.name.message as string}</span>}
      </div>

      <div style={{ marginTop: '10px' }}>
        <label htmlFor="file">Currículo (PDF)</label>
        <input 
          id="file" 
          type="file" 
          accept=".pdf"
          {...register('file')} 
          disabled={isSubmitting}
        />
        {errors.file && <span style={{ color: 'red', display: 'block' }}>{errors.file.message as string}</span>}
      </div>

      <button type="submit" disabled={isSubmitting} style={{ marginTop: '15px' }}>
        {isSubmitting ? 'A enviar...' : 'Enviar'}
      </button>
    </form>
  );
};

export default ResumeForm;