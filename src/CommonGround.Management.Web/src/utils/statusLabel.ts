const STATUS_LABELS: Record<string, string> = {
  Available: 'Available',
  Predevelopment: 'Predevelopment',
  UnderConstruction: 'Under Construction',
  ReadyForOccupancy: 'Ready for Occupancy',
  Transitioning: 'Transitioning',
  Decommissioned: 'Decommissioned',
  Occupied: 'Occupied',
  InSelection: 'In Selection',
  HeldByTrust: 'Held by Trust',
  FellThrough: 'Fell Through',
  UnderDevelopment: 'Under Development',
  UnderRenovation: 'Under Renovation',
}

export function statusLabel(status: string): string {
  return STATUS_LABELS[status] ?? status.replace(/([a-z])([A-Z])/g, '$1 $2')
}
