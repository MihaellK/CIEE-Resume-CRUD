import React from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';

// 1. Atualização do schema para incluir a validação do FileList
const schema = z.object({
  name: z.string().min(1, 'O nome do candidato é obrigatório'),
  file: z.any()
    .refine((files) => files && files.length > 0, 'O currículo em PDF é obrigatório')
});

// Inferência de tipos automática a partir do schema
type ResumeFormData = z.infer<typeof schema>;

const ResumeForm: React.FC = () => {
  // 2. Configuração do React Hook Form com o resolver do Zod
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResumeFormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = (data: ResumeFormData) => {
    // A extração real do arquivo (data.file[0]) será feita aqui na integração com a API
    console.log("Formulário submetido com sucesso", data);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div>
        <label htmlFor="name">Nome do Candidato</label>
        <input 
          id="name" 
          {...register('name')} 
          placeholder="Digite seu nome"
        />
        {errors.name && <span style={{ color: 'red', display: 'block' }}>{errors.name.message as string}</span>}
      </div>

      {/* 2. Adição do input de arquivo */}
      <div style={{ marginTop: '10px' }}>
        <label htmlFor="file">Currículo (PDF)</label>
        <input 
          id="file" 
          type="file" 
          accept=".pdf"
          {...register('file')} 
        />
        {errors.file && <span style={{ color: 'red', display: 'block' }}>{errors.file.message as string}</span>}
      </div>

      <button type="submit" style={{ marginTop: '15px' }}>Enviar</button>
    </form>
  );
};

export default ResumeForm;