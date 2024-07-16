import React, { useState, FormEvent } from 'react';

interface PseudoCodeFormProps {
    onSubmit: (pseudocode: string) => void;
}

const PseudoCodeForm: React.FC<PseudoCodeFormProps> = ({ onSubmit }) => {
    const [pseudocode, setPseudocode] = useState<string>('');

    const handleSubmit = (e: FormEvent) => {
        e.preventDefault();
        onSubmit(pseudocode);
    };

    return (
        <form onSubmit={handleSubmit}>
            <textarea
                value={pseudocode}
                onChange={(e) => setPseudocode(e.target.value)}
                rows={10}
                cols={50}
                placeholder="Escribe tu pseudocodigo aqui..."
            />
            <button type="submit">Compilar</button>
        </form>
    );
};

export default PseudoCodeForm;