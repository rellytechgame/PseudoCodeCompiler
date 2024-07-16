import React, { useState } from 'react';
import PseudoCodeForm from './components/PseudoCodeForm';
import Result from './components/Result';
import './App.css';

interface CompileResult {
    isSuccess: boolean;
    output?: string;
    errors?: string;
}

const App: React.FC = () => {
    const [result, setResult] = useState<CompileResult | null>(null);

    const compilePseudocode = async (pseudocode: string) => {
        const response = await fetch('https://localhost:7049/compile', { // Cambia esta URL según sea necesario
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ pseudocode }),
        });

        const data: CompileResult = await response.json();
        setResult(data);
    };

    return (
        <div className="App">
            <h1>Compilador de Pseudocodigo</h1>
            <PseudoCodeForm onSubmit={compilePseudocode} />
            <Result result={result} />
        </div>
    );
};

export default App;