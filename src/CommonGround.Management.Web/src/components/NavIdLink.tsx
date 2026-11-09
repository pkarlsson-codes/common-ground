import { useNavigate } from 'react-router-dom'

interface Props {
  id?: string | null
  label: string
  to: string
}

export default function NavIdLink({ id, label, to }: Props) {
  const navigate = useNavigate()

  if (!id) {
    return null
  }

  return (
    <button
      type="button"
      className="btn btn-link btn-sm"
      onClick={() => navigate(to, { state: { openId: id } })}
      aria-label={label}
    >
      {label}
    </button>
  )
}