export interface UiTelemetryEvent {
  event: string
  aggregate: 'parcel' | 'building' | 'workflow' | 'lease'
  actionId: string
  entityId?: string
  fromState?: string | null
  toState?: string | null
  outcome: 'attempt' | 'success' | 'failure'
  error?: string
}

export function trackTransitionEvent(payload: UiTelemetryEvent) {
  if (typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent('cg:transition-event', { detail: payload }))
  }

  // Keep telemetry pluggable while preserving useful debug output in dev.
  console.info('[cg.transition]', payload)
}
