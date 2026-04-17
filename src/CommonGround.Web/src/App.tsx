import './App.css'

function App() {
  return (
    <main className="home">
      <header className="home__header">
        <p className="kicker">CommonGround</p>
        <h1>Web App Scaffold Ready</h1>
        <p>
          React + Vite + TanStack Query is configured. API requests can be proxied through the gateway at
          <code> /api </code>
          during local development.
        </p>
      </header>

      <section className="home__grid" aria-label="Development quick links">
        <article className="card">
          <h2>Run Frontend</h2>
          <p>Use npm scripts from the repository root:</p>
          <pre><code>npm run frontend:start</code></pre>
        </article>

        <article className="card">
          <h2>Generate API Client</h2>
          <p>Generate TypeScript clients from the gateway OpenAPI spec:</p>
          <pre><code>npm run frontend:generate</code></pre>
        </article>

        <article className="card">
          <h2>Backend Gateway</h2>
          <p>Expected local gateway endpoint:</p>
          <pre><code>http://localhost:4000</code></pre>
        </article>
      </section>
    </main>
  )
}

export default App
