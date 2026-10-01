import React from 'react';
import ResumeForm from './components/ResumeForm';

const App: React.FC = () => {
  return (
    <div style={{ maxWidth: '600px', margin: '0 auto', padding: '2rem', fontFamily: 'sans-serif' }}>
      <h1>Cadastro de Currículos</h1>
      <ResumeForm />
    </div>
  );
};

export default App;