import { FormEvent, useState } from 'react'

type SignupResult = {
  trustId: string
  nextUrl: string
}

export default function SignupPage() {
  const [name, setName] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const trimmed = name.trim()
    if (!trimmed) {
      setError('Please enter a community land trust name.')
      return
    }

    setIsSubmitting(true)
    setError(null)

    try {
      const response = await fetch('/auth/signup', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: trimmed }),
      })

      if (!response.ok) {
        setError('Could not create trust. Please try again.')
        return
      }

      const payload = (await response.json()) as SignupResult
      window.location.href = payload.nextUrl
    } catch {
      setError('Could not reach the server. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: 'var(--bg)',
      padding: '24px',
    }}>
      <form
        onSubmit={onSubmit}
        style={{
          textAlign: 'left',
          padding: '42px 44px',
          background: 'var(--surface)',
          border: '1px solid var(--border)',
          borderRadius: '12px',
          maxWidth: '440px',
          width: '100%',
          display: 'flex',
          flexDirection: 'column',
          gap: '18px',
        }}
      >
        <div>
          <div style={{
            fontFamily: 'var(--font-mono)',
            fontSize: '9px',
            letterSpacing: '0.22em',
            textTransform: 'uppercase',
            color: 'var(--amber)',
            marginBottom: '10px',
          }}>
            New CLT
          </div>
          <h1 style={{
            margin: 0,
            fontFamily: 'var(--font-display)',
            fontSize: '30px',
            lineHeight: 1.15,
            color: 'var(--text-bright)',
          }}>
            Create your trust
          </h1>
          <p style={{
            margin: '10px 0 0 0',
            fontSize: '13px',
            color: 'var(--text-dim)',
            lineHeight: 1.5,
          }}>
            Start with your organization name. You will continue to account registration next.
          </p>
        </div>

        <label style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
          <span style={{ fontSize: '12px', letterSpacing: '0.05em', textTransform: 'uppercase', color: 'var(--text-dim)' }}>
            Trust Name
          </span>
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder='Example Community Land Trust'
            autoFocus
            disabled={isSubmitting}
            style={{
              width: '100%',
              padding: '12px 14px',
              borderRadius: '8px',
              border: '1px solid var(--border)',
              background: 'transparent',
              color: 'var(--text-bright)',
              fontSize: '14px',
              fontFamily: 'var(--font-body)',
            }}
          />
        </label>

        {error && (
          <div style={{ color: 'var(--error-bright)', fontSize: '13px' }}>{error}</div>
        )}

        <button
          type='submit'
          disabled={isSubmitting}
          style={{
            width: '100%',
            padding: '11px 20px',
            background: 'var(--amber)',
            color: 'var(--bg)',
            border: 'none',
            borderRadius: '6px',
            fontFamily: 'var(--font-body)',
            fontSize: '13.5px',
            fontWeight: 600,
            cursor: isSubmitting ? 'default' : 'pointer',
            letterSpacing: '0.02em',
            opacity: isSubmitting ? 0.7 : 1,
          }}
        >
          {isSubmitting ? 'Creating trust…' : 'Create trust and continue'}
        </button>

        <a
          href='/login'
          style={{
            textAlign: 'center',
            fontSize: '13px',
            color: 'var(--text-dim)',
            textDecoration: 'none',
          }}
        >
          Back to login options
        </a>
      </form>
    </div>
  )
}