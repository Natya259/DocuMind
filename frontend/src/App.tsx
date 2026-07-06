import { useState } from 'react'
import axios from 'axios'
import './App.css'

function App() {

  const [statusCode, setStatusCode] = useState<number | null>(null);

  const handleClick = async () => {
    try {
      const inputElement = document.getElementById("fileInput") as HTMLInputElement;
      const formData = new FormData();
      if (inputElement.files) {
        for (const file of inputElement.files) {
          formData.append("Files", file);
        }
      }

      const docsResponse = await axios.post("https://localhost:7218/api/documents",
        formData
      );
      setStatusCode(docsResponse.status);

      console.log(docsResponse.status);
    } catch (error) {
      console.error('Error uploading documents:', error);
    }
  };


  return (
    <>
      <section id="center">

        <div>
          <h1>Get started</h1>
          <p>
            Upload your documents and let our AI analyze them for you...
          </p>
        </div>

        <div>
          <input type="file" id="fileInput" multiple />
        </div>

        <button
          type="button"
          className="counter"
          onClick={handleClick}
        >
          Upload documents
        </button>
      </section>
      {
        statusCode !== null && (
          <div>
            <h2>Upload Response:</h2>
            <p>Status Code: {statusCode}</p>
          </div>
        )
      }

      <div className="ticks"></div>

      <div className="ticks"></div>
      <section id="spacer"></section>
    </>
  )
}

export default App
