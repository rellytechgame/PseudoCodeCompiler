import React from 'react';

interface ResultProps {
    result: {
        isSuccess: boolean;
        output?: string;
        errors?: string;
    } | null;
}

const Result: React.FC<ResultProps> = ({ result }) => {
    if (!result) {
        return null;
    }

    return (
        <div>
            {result.isSuccess ? (
                <div className="success">
                    <h2>Resultado:</h2>
                    <pre>{result.output}</pre>
                </div>
            ) : (
                <div className="error">
                    <h2>Errores:</h2>
                    <pre>{result.errors}</pre>
                </div>
            )}
        </div>
    );
};

export default Result;