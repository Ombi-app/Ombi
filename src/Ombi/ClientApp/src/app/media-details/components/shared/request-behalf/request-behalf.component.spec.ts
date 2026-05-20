import { describe, it, expect, vi } from 'vitest';
import { RequestBehalfComponent } from './request-behalf.component';
import { of } from 'rxjs';

function createComponent() {
  const mockDialogRef = { close: vi.fn() };
  const mockIdentity = {
    getUsersDropdown: vi.fn().mockReturnValue(of([
      { id: '1', username: 'admin', email: 'admin@test.com' },
      { id: '2', username: 'user1', email: 'user@test.com' },
    ])),
  };

  const comp = new RequestBehalfComponent(mockDialogRef as any, mockIdentity as any);
  return { comp, mockDialogRef, mockIdentity };
}

describe('RequestBehalfComponent', () => {
  it('should load user options on init', async () => {
    const { comp, mockIdentity } = createComponent();
    await comp.ngOnInit();
    expect(mockIdentity.getUsersDropdown).toHaveBeenCalled();
    expect(comp.options).toHaveLength(2);
  });

  it('should close dialog with selected value on request', () => {
    const { comp, mockDialogRef } = createComponent();
    comp.myControl.setValue({ id: '1', username: 'admin' });
    comp.request();
    expect(mockDialogRef.close).toHaveBeenCalledWith({ id: '1', username: 'admin' });
  });

  it('should close dialog on onNoClick', () => {
    const { comp, mockDialogRef } = createComponent();
    comp.onNoClick();
    expect(mockDialogRef.close).toHaveBeenCalled();
  });

  it('should format display name with username and email', () => {
    const { comp } = createComponent();
    expect(comp.displayFn({ username: 'admin', email: 'admin@test.com' } as any)).toBe('admin (admin@test.com)');
  });

  it('should format display name with only username when no email', () => {
    const { comp } = createComponent();
    expect(comp.displayFn({ username: 'admin' } as any)).toBe('admin ');
  });

  it('should return empty string for null user', () => {
    const { comp } = createComponent();
    expect(comp.displayFn(null as any)).toBe(' ');
  });
});
