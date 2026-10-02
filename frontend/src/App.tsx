import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import ResumeForm from './components/ResumeForm';
import ResumeList from './components/ResumeList'; // Assumo que este é o seu componente de listagem
import ResumeDetails from './components/ResumeDetails';

// Criamos um componente agregador para a página principal
const Home: React.FC = () => (
  <>
    <ResumeForm />
    <hr style={{ margin: '3rem 0', border: 'none', borderTop: '1px solid #eee' }} />
    <ResumeList />
  </>
);

const App: React.FC = () => {
  return (
    <Router>
      <div style={{ maxWidth: '800px', margin: '0 auto', padding: '2rem', fontFamily: 'sans-serif' }}>
        <h1 style={{ textAlign: 'center', color: '#333' }}>Gestão de Currículos</h1>
        
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/resumes/:id" element={<ResumeDetails />} />
        </Routes>
      </div>
    </Router>
  );
};

export default App;