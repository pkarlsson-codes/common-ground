export type TransitionAggregate = 'parcel' | 'building' | 'workflow' | 'lease'

export interface TransitionAction {
  id: string
  label: string
  toState?: string
  requiresConfirmation?: boolean
  confirmationMessage?: string
}

type TransitionMap = Record<string, TransitionAction[]>

const parcelTransitions: TransitionMap = {
  Available: [
    { id: 'parcel:start-development', label: 'Start Development', toState: 'UnderDevelopment' },
    { id: 'parcel:mark-occupied', label: 'Mark Occupied', toState: 'Occupied' },
  ],
  UnderDevelopment: [
    { id: 'parcel:mark-available', label: 'Mark Available', toState: 'Available' },
    { id: 'parcel:mark-occupied', label: 'Mark Occupied', toState: 'Occupied' },
  ],
  Occupied: [
    { id: 'parcel:mark-available', label: 'Mark Available', toState: 'Available' },
    { id: 'parcel:start-redevelopment', label: 'Start Redevelopment', toState: 'UnderDevelopment' },
  ],
}

const buildingTransitions: TransitionMap = {
  Available: [
    { id: 'building:mark-occupied', label: 'Mark Occupied', toState: 'Occupied' },
    { id: 'building:start-renovation', label: 'Start Renovation', toState: 'UnderRenovation' },
  ],
  Occupied: [
    { id: 'building:mark-available', label: 'Mark Available', toState: 'Available' },
    { id: 'building:start-renovation', label: 'Start Renovation', toState: 'UnderRenovation' },
  ],
  UnderRenovation: [
    { id: 'building:mark-available', label: 'Mark Available', toState: 'Available' },
    { id: 'building:mark-occupied', label: 'Mark Occupied', toState: 'Occupied' },
  ],
}

const workflowTransitions: TransitionMap = {
  Initiated: [
    { id: 'workflow:record-tracks', label: 'Record Purchase Tracks' },
    {
      id: 'workflow:fail',
      label: 'Fail Workflow',
      requiresConfirmation: true,
      confirmationMessage: 'Mark this workflow as failed? This cannot be undone.',
    },
    {
      id: 'workflow:withdraw',
      label: 'Withdraw Application',
      requiresConfirmation: true,
      confirmationMessage: 'Withdraw this workflow? This cannot be undone.',
    },
  ],
  InProgress: [
    { id: 'workflow:record-tracks', label: 'Record Purchase Tracks' },
    { id: 'workflow:schedule-closing', label: 'Schedule Closing' },
    { id: 'workflow:stall', label: 'Stall Workflow' },
    {
      id: 'workflow:fail',
      label: 'Fail Workflow',
      requiresConfirmation: true,
      confirmationMessage: 'Mark this workflow as failed? This cannot be undone.',
    },
    {
      id: 'workflow:withdraw',
      label: 'Withdraw Application',
      requiresConfirmation: true,
      confirmationMessage: 'Withdraw this workflow? This cannot be undone.',
    },
  ],
  Stalled: [
    { id: 'workflow:record-tracks', label: 'Record Purchase Tracks' },
    { id: 'workflow:schedule-closing', label: 'Schedule Closing' },
    {
      id: 'workflow:fail',
      label: 'Fail Workflow',
      requiresConfirmation: true,
      confirmationMessage: 'Mark this workflow as failed? This cannot be undone.',
    },
    {
      id: 'workflow:withdraw',
      label: 'Withdraw Application',
      requiresConfirmation: true,
      confirmationMessage: 'Withdraw this workflow? This cannot be undone.',
    },
  ],
  ClosingScheduled: [
    { id: 'workflow:record-closed', label: 'Record Closing' },
    {
      id: 'workflow:fail',
      label: 'Fail Workflow',
      requiresConfirmation: true,
      confirmationMessage: 'Mark this workflow as failed? This cannot be undone.',
    },
    {
      id: 'workflow:withdraw',
      label: 'Withdraw Application',
      requiresConfirmation: true,
      confirmationMessage: 'Withdraw this workflow? This cannot be undone.',
    },
  ],
  Closed: [],
  Failed: [],
  Withdrawn: [],
}

const leaseTransitions: TransitionMap = {
  Active: [
    {
      id: 'lease:terminate',
      label: 'Terminate Lease',
      toState: 'Inactive',
      requiresConfirmation: true,
      confirmationMessage: 'Terminate this lease? This cannot be undone.',
    },
  ],
  Inactive: [],
}

const transitionMaps: Record<TransitionAggregate, TransitionMap> = {
  parcel: parcelTransitions,
  building: buildingTransitions,
  workflow: workflowTransitions,
  lease: leaseTransitions,
}

export function getAvailableActions(aggregate: TransitionAggregate, currentState?: string | null): TransitionAction[] {
  if (!currentState) {
    return []
  }

  return transitionMaps[aggregate][currentState] ?? []
}
