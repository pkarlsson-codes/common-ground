const STATUS_LABELS: Record<string, string> = {
  Available: 'Available',
  Predevelopment: 'Predevelopment',
  UnderConstruction: 'Under Construction',
  ReadyForOccupancy: 'Ready for Occupancy',
  Transitioning: 'Transitioning',
  Decommissioned: 'Decommissioned',
  Occupied: 'Occupied',
  UnderDevelopment: 'Under Development',
  UnderRenovation: 'Under Renovation',
}

export function statusLabel(status: string): string {
  return STATUS_LABELS[status] ?? status
}
