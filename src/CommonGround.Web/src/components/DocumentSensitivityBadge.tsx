interface Props {
  sensitivity: string
}

const SENSITIVITY_STYLES: Record<string, { bg: string; color: string; label: string }> = {
  Public:       { bg: 'var(--green-bg)',  color: 'var(--green-text)',  label: 'Public' },
  Internal:     { bg: 'var(--sky-bg)',    color: 'var(--sky-text)',    label: 'Internal' },
  Confidential: { bg: 'var(--orange-bg)', color: 'var(--orange-text)', label: 'Confidential' },
  Restricted:   { bg: 'var(--red-dim)',   color: 'var(--red)',         label: 'Restricted' },
}

export default function DocumentSensitivityBadge({ sensitivity }: Props) {
  const style = SENSITIVITY_STYLES[sensitivity] ?? {
    bg: 'rgba(120,140,160,.12)',
    color: 'var(--text-secondary)',
    label: sensitivity,
  }

  const isRestricted = sensitivity === 'Restricted'

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 4,
        padding: '2px 8px',
        borderRadius: 4,
        fontSize: 11,
        fontFamily: 'var(--font-mono)',
        letterSpacing: '0.04em',
        textTransform: 'uppercase',
        background: style.bg,
        color: style.color,
        fontWeight: 600,
      }}
    >
      {isRestricted && (
        <svg width="10" height="10" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
          <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
        </svg>
      )}
      {style.label}
    </span>
  )
}
