import { useEffect, useRef } from 'react'

// Open overlays in the order they opened. Only the last one answers Escape.
const stack: symbol[] = []

export function useOverlayEscape(active: boolean, onEscape: () => void) {
  const handler = useRef(onEscape)
  useEffect(() => { handler.current = onEscape })

  useEffect(() => {
    if (!active) return
    const id = Symbol('overlay')
    stack.push(id)
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && stack[stack.length - 1] === id) handler.current()
    }
    window.addEventListener('keydown', onKey)
    return () => {
      window.removeEventListener('keydown', onKey)
      stack.splice(stack.indexOf(id), 1)
    }
  }, [active])
}
