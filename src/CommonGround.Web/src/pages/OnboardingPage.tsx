export default function OnboardingPage() {
  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'var(--bg)',
        padding: '24px',
      }}
    >
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: '1fr 1fr',
          gap: '64px',
          maxWidth: '900px',
          width: '100%',
          alignItems: 'center',
        }}
      >
        {/* Left: explanation */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <div
            style={{
              fontFamily: 'var(--font-mono)',
              fontSize: '9px',
              letterSpacing: '0.22em',
              textTransform: 'uppercase',
              color: 'var(--amber)',
            }}
          >
            CommonGround
          </div>
          <h1
            style={{
              margin: 0,
              fontFamily: 'var(--font-display)',
              fontSize: '34px',
              lineHeight: 1.1,
              color: 'var(--text-bright)',
            }}
          >
            You're not part of a trust yet
          </h1>
          <p style={{ margin: 0, fontSize: '14px', color: 'var(--text-secondary)', lineHeight: 1.6 }}>
            CommonGround is organized around Community Land Trusts. To access the platform, you either need an
            invite from a trust admin, or you can create your own trust.
          </p>
          <p style={{ margin: 0, fontSize: '13px', color: 'var(--text-dim)', lineHeight: 1.5 }}>
            If you've received an invite, just open the link from your email while signed in and you'll be
            added automatically.
          </p>
        </div>

        {/* Right: create a trust CTA */}
        <div
          style={{
            padding: '36px 40px',
            background: 'var(--surface)',
            border: '1px solid var(--border)',
            borderRadius: '12px',
            display: 'flex',
            flexDirection: 'column',
            gap: '20px',
          }}
        >
          <div>
            <div
              style={{
                fontFamily: 'var(--font-mono)',
                fontSize: '9px',
                letterSpacing: '0.22em',
                textTransform: 'uppercase',
                color: 'var(--text-dim)',
                marginBottom: '8px',
              }}
            >
              New trust
            </div>
            <p style={{ margin: 0, fontSize: '14px', color: 'var(--text-bright)', lineHeight: 1.5 }}>
              Starting a new Community Land Trust? Create one here and invite your team.
            </p>
          </div>
          <a
            href='/signup'
            style={{
              display: 'inline-flex',
              justifyContent: 'center',
              padding: '10px 20px',
              background: 'var(--amber)',
              color: 'var(--bg)',
              border: 'none',
              borderRadius: '6px',
              fontFamily: 'var(--font-body)',
              fontSize: '13.5px',
              fontWeight: 600,
              letterSpacing: '0.02em',
              textDecoration: 'none',
              cursor: 'pointer',
            }}
          >
            Create a trust
          </a>
        </div>
      </div>
    </div>
  )
}
