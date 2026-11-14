import { useEffect, useState } from 'react'

/** Waits until the value has stopped changing for a moment, so typing a figure asks for one price, not one per key. */
export function useSettled<T>(value: T, ms = 300): T {
  const [settled, setSettled] = useState(value)
  useEffect(() => {
    const timer = setTimeout(() => setSettled(value), ms)
    return () => clearTimeout(timer)
  }, [value, ms])
  return settled
}
