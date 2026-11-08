import { useSlideOverClose } from './slideOverClose'

export default function SlideOverCancelButton() {
  const requestClose = useSlideOverClose()
  return <button type="button" className="btn btn-ghost" onClick={requestClose}>Cancel</button>
}
