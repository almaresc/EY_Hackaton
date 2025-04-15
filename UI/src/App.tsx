import React, { useState } from 'react';

const App: React.FC = () => {
    const [visFile, setVisFile] = useState<File | null>(null);
    const [canvasFile, setCanvasFile] = useState<File | null>(null);
    const [uploaded, setUploaded] = useState<Boolean>(false);

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>, setFile: React.Dispatch<React.SetStateAction<File | null>>) => {
        if (event.target.files && event.target.files[0]) {
            setFile(event.target.files[0]);
        }
    };

    const handleSubmit = async () => {
        if (!visFile || !canvasFile) {
            alert('Please upload both VIS and CANVAS files.');
            return;
        }

        const formData = new FormData();
        formData.append('vis', visFile);
        formData.append('canvas', canvasFile);

        try {
            const response = await fetch('https://api.documentintelligence.com/ocr', {
                method: 'POST',
                body: formData,
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const result = await response.json();
            console.log('OCR Result:', result);
        } catch (error) {
            console.error('Error during OCR:', error);
        }
    };

    return (
        <>
            { !uploaded && (
                <div>
                    <h1>File Upload</h1>
                    <div>
                        <label>
                            VIS File:
                            <input type="file" onChange={(e) => handleFileChange(e, setVisFile)} />
                        </label>
                    </div>
                    <div>
                        <label>
                            CANVAS File:
                            <input type="file" onChange={(e) => handleFileChange(e, setCanvasFile)} />
                        </label>
                    </div>
                    <button onClick={handleSubmit}>Upload and Process</button>
                </div>
            )}
            {uploaded && (
                <div>Test chat       </div>
            )}
        </>
      
    );
};

export default App;
