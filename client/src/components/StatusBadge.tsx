
import { Badge } from './Badge';

export interface StatusBadgeProps {
  status: string;
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const getStatusConfig = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active':
      case 'نشط':
      case 'مكتمل':
        return { variant: 'success' as const, label: 'نشط' };
      case 'pending':
      case 'قيد الانتظار':
        return { variant: 'warning' as const, label: 'قيد الانتظار' };
      case 'error':
      case 'فشل':
      case 'مرفوض':
        return { variant: 'error' as const, label: 'فشل' };
      default:
        return { variant: 'default' as const, label: status };
    }
  };

  const config = getStatusConfig(status);

  return <Badge variant={config.variant}>{config.label}</Badge>;
}
