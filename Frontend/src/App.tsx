import "./App.css";
// @deno-types="@types/react"
import { useState } from "react";
// @ts-expect-error Unable to infer type at the moment
import reactLogo from "./assets/react.svg";

function App() {
  const [count, setCount] = useState(0);

  const sendMessage = () => {
    // @ts-ignore
    window.external.sendMessage("Hello from React!");
  };
  const getGreeting = async () => {
    const response = await fetch("http://localhost:5000/greetings");
    const data = await response.text();
    alert(data);
  };

  return (
    <>
      <img src="/vite-deno.svg" alt="Vite with Deno" />
      <div>
        <a href="https://vite.dev" target="_blank">
          <img src="/vite.svg" className="logo" alt="Vite logo" />
        </a>
        <a href="https://reactjs.org" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>Vite + React</h1>
      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
        <p>HMR test 3</p>
        <button onClick={sendMessage}>
          Send message
        </button>
        <button onClick={getGreeting}>
          Get Greeting
        </button>
      </div>
      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>
    </>
  );
}

export default App;
