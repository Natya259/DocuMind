import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from './assets/vite.svg'
import heroImg from './assets/hero.png'
import './App.css'

function App() {

  const [statusCode, setStatusCode] = useState<number | null>(null);
  
  const handleClick = async () => {
    try {
      const response = await fetch('https://localhost:7218/health');
      setStatusCode(response.status);
      console.log(response);
    } catch (error) {
      console.error('Error fetching health check:', error);
    }
  };

  return (
    <>
      <section id="center">
        
        <div>
          <h1>Get started</h1>
          <p>
            Click button to call health check API.
          </p>
        </div>
        <button
          type="button"
          className="counter"
          onClick={handleClick}
        >
          Health Check
        </button>
      </section>
      {
        statusCode !== null && (
          <div>
            <h2>Health Check Response:</h2>
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
