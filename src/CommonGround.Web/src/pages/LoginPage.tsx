export default function LoginPage() {
  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: 'var(--bg)',
    }}>
      <div style={{
        textAlign: 'center',
        padding: '48px 56px',
        background: 'var(--surface)',
        border: '1px solid var(--border)',
        borderRadius: '12px',
        maxWidth: '360px',
        width: '100%',
      }}>
        <div style={{
          fontFamily: 'var(--font-mono)',
          fontSize: '9px',
          letterSpacing: '0.22em',
          textTransform: 'uppercase',
          color: 'var(--amber)',
          marginBottom: '8px',
        }}>
          CLT Registry
        </div>
        <div style={{
          fontFamily: 'var(--font-display)',
          fontSize: '32px',
          fontWeight: 600,
          color: 'var(--text-bright)',
          letterSpacing: '-0.01em',
          lineHeight: 1.1,
          marginBottom: '6px',
        }}>
          CommonGround
        </div>
        <div style={{
          fontSize: '11px',
          color: 'var(--text-dim)',
          letterSpacing: '0.08em',
          textTransform: 'uppercase',
          marginBottom: '36px',
        }}>
          Property Register
        </div>
        <button
          onClick={() => { window.location.href = '/auth/login' }}
          style={{
            width: '100%',
            padding: '10px 20px',
            background: 'var(--amber)',
            color: 'var(--bg)',
            border: 'none',
            borderRadius: '6px',
            fontFamily: 'var(--font-body)',
            fontSize: '13.5px',
            fontWeight: 600,
            cursor: 'pointer',
            letterSpacing: '0.02em',
          }}
        >
          Log in
        </button>
      </div>
    </div>
  )
}
