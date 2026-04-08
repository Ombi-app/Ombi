import { describe, it, expect, vi, beforeEach } from 'vitest';
import { NotificationService } from './notification.service';

function createService() {
  const mockSnackBar = {
    open: vi.fn(),
  };

  const service = new (NotificationService as any)(mockSnackBar);

  return { service: service as NotificationService, mockSnackBar };
}

describe('NotificationService', () => {
  let service: NotificationService;
  let mockSnackBar: { open: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockSnackBar = mocks.mockSnackBar;
  });

  it('should open snackbar on success', () => {
    service.success('Request approved');
    expect(mockSnackBar.open).toHaveBeenCalledWith('Request approved', 'OK', expect.objectContaining({ duration: 3000 }));
  });

  it('should open snackbar on error', () => {
    service.error('Something went wrong');
    expect(mockSnackBar.open).toHaveBeenCalledWith('Something went wrong', 'OK', expect.objectContaining({ duration: 3000 }));
  });

  it('should open snackbar on info', () => {
    service.info('Info', 'Some info message');
    expect(mockSnackBar.open).toHaveBeenCalledWith('Some info message', 'OK', expect.objectContaining({ duration: 3000 }));
  });

  it('should open snackbar on warning', () => {
    service.warning('Warning', 'Watch out');
    expect(mockSnackBar.open).toHaveBeenCalledWith('Watch out', 'OK', expect.objectContaining({ duration: 3000 }));
  });
});
