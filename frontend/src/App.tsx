import React from 'react';
import ResumeForm from './components/ResumeForm';
import ResumeList from './components/ResumeList';

const App: React.FC = () => {
  return (
    <div style={{ maxWidth: '800px', margin: '0 auto', padding: '2rem', fontFamily: 'sans-serif' }}>
      <h1>Gestão de Currículos</h1>
      <ResumeForm />
      <ResumeList />
    </div>
  );
};

export default App;