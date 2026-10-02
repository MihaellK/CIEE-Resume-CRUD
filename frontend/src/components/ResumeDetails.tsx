import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import axios from 'axios';

interface Resume {
  id: string;
  name: string;
  email: string;
  phone?: string;
  areaOfInterest?: string;
  professionalSummary?: string;
}

const ResumeDetails: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [resume, setResume] = useState<Resume | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchResume = async () => {
      try {
        const baseUrl = import.meta.env.VITE_API_URL 
          ? import.meta.env.VITE_API_URL.replace('/upload', '') 
          : 'http://localhost:5092/api/resumes';

        const response = await axios.get(`${baseUrl}/${id}`);
        setResume(response.data);
      } catch (err: unknown) {
        console.error(err);
        if (axios.isAxiosError(err) && err.response?.status === 404) {
          setError('Currículo não encontrado (404).');
        } else {
          setError('Ocorreu um erro ao carregar os detalhes do currículo.');
        }
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchResume();
    }
  }, [id]);

  if (loading) return <div style={{ marginTop: '2rem' }}>Carregando...</div>;
  if (error) return <div style={{ color: 'red', marginTop: '2rem' }}>{error}</div>;
  if (!resume) return <div style={{ marginTop: '2rem' }}>Currículo não encontrado.</div>;

  return (
    <div style={{ marginTop: '2rem' }}>
      <Link to="/" style={{ textDecoration: 'none', color: '#0066cc', marginBottom: '1rem', display: 'inline-block' }}>
        &larr; Voltar para a lista
      </Link>
      
      <div style={{ padding: '2rem', backgroundColor: '#fff', border: '1px solid #e0e0e0', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0,0.05)' }}>
        <h2 style={{ marginTop: 0, borderBottom: '2px solid #f0f0f0', paddingBottom: '0.5rem' }}>{resume.name}</h2>
        
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginTop: '1.5rem' }}>
          <div>
            <strong style={{ display: 'block', fontSize: '0.85rem', color: '#666' }}>E-mail</strong>
            <span>{resume.email}</span>
          </div>
          
          <div>
            <strong style={{ display: 'block', fontSize: '0.85rem', color: '#666' }}>Telefone</strong>
            <span>{resume.phone || 'Não informado'}</span>
          </div>
          
          <div style={{ gridColumn: '1 / -1' }}>
            <strong style={{ display: 'block', fontSize: '0.85rem', color: '#666' }}>Área ou Cargo de Interesse</strong>
            <span>{resume.areaOfInterest || 'Não informado'}</span>
          </div>

          <div style={{ gridColumn: '1 / -1', marginTop: '1rem' }}>
            <strong style={{ display: 'block', fontSize: '0.85rem', color: '#666' }}>Resumo Profissional</strong>
            {resume.professionalSummary ? (
              <p style={{ whiteSpace: 'pre-wrap', margin: '0.5rem 0 0 0', lineHeight: '1.5' }}>
                {resume.professionalSummary}
              </p>
            ) : (
              <span style={{ color: '#999' }}>Nenhum resumo profissional fornecido.</span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ResumeDetails;