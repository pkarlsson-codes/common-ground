import { describe, expect, it } from 'vitest'
import type { HouseholdResponse } from '../api'
import { findOpenApplication } from './households'

const householdWith = (...phases: string[]) =>
  ({ applications: phases.map((phase, i) => ({ id: `app-${i}`, phase })) }) as unknown as HouseholdResponse

describe('findOpenApplication', () => {
  it.each(['Submitted', 'Approved', 'Selected'])('finds a %s application', phase => {
    expect(findOpenApplication(householdWith('Withdrawn', phase))?.phase).toBe(phase)
  })

  it('finds nothing when every application is closed', () => {
    expect(findOpenApplication(householdWith('Denied', 'Withdrawn', 'Purchased'))).toBeUndefined()
  })
})
