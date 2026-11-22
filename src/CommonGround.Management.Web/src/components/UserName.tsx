import { useUserName } from '../hooks/useUserName'

/** A user id shown as the member's name. */
export default function UserName({ id }: { id?: string | null }) {
  const nameOf = useUserName()
  return <>{nameOf(id)}</>
}
