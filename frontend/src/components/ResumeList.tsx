import React, { useEffect, useState } from 'react';
import axios from 'axios';

interface Resume {
  id: string;
  name: string;
  email: string | null;
  phone: string | null;
}

const ResumeList: React.FC = () => {
  const [resumes, setResumes] = useState<Resume[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // AbortController previne atualizações de estado em componentes desmontados
    const abortController = new AbortController();

    const fetchResumes = async () => {
      try {
        const baseUrl = import.meta.env.VITE_API_URL 
          ? import.meta.env.VITE_API_URL.replace('/upload', '') 
          : 'http://localhost:5092/api/resumes';
          
        const response = await axios.get(baseUrl, {
          signal: abortController.signal
        });
        
        setResumes(response.data);
      } catch (err) {
        if (axios.isCancel(err)) {
          return; // Ignora o erro se foi causado pelo cancelamento do componente
        }
        setError('Erro ao carregar os currículos.');
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchResumes();

    // Função de cleanup do useEffect
    return () => {
      abortController.abort();
    };
  }, []);

  if (isLoading) return <p>A carregar...</p>;
  if (error) return <p style={{ color: 'red' }}>{error}</p>;
  if (resumes.length === 0) return <p>Nenhum currículo encontrado.</p>;

  return (
    <div style={{ marginTop: '2rem' }}>
      <hr style={{ marginBottom: '2rem', borderColor: '#eee' }} />
      <h2>Currículos Cadastrados</h2>
      
      <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '1rem' }}>
        <thead>
          <tr style={{ borderBottom: '2px solid #ccc', textAlign: 'left' }}>
            <th style={{ padding: '0.5rem' }}>Nome</th>
            <th style={{ padding: '0.5rem' }}>E-mail</th>
            <th style={{ padding: '0.5rem' }}>Telefone</th>
          </tr>
        </thead>
        <tbody>
          {resumes.map((resume) => (
            <tr key={resume.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.5rem' }}>{resume.name}</td>
              <td style={{ padding: '0.5rem' }}>{resume.email || 'Não informado'}</td>
              <td style={{ padding: '0.5rem' }}>{resume.phone || 'Não informado'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default ResumeList;