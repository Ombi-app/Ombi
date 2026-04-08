import { describe, it, expect, vi } from 'vitest';
import { DenyDialogComponent } from './deny-dialog.component';
import { RequestType } from '../../../../interfaces';
import { of } from 'rxjs';

function createComponent(requestType = RequestType.movie) {
  const mockDialogRef = { close: vi.fn() };
  const data = { requestId: 42, is4K: false, requestType, denied: false };
  const mockRequestService = {
    denyMovie: vi.fn().mockReturnValue(of({ result: true })),
    denyChild: vi.fn().mockReturnValue(of({ result: true })),
    denyAlbum: vi.fn().mockReturnValue(of({ result: true })),
  };
  const mockMessageService = { send: vi.fn(), sendRequestEngineResultError: vi.fn() };
  const mockTranslate = { instant: vi.fn((key: string) => key) };

  const comp = new DenyDialogComponent(mockDialogRef as any, data, mockRequestService as any, mockMessageService as any, mockTranslate as any);
  return { comp, mockDialogRef, data, mockRequestService, mockMessageService };
}

describe('DenyDialogComponent', () => {
  it('should deny movie request', async () => {
    const { comp, mockRequestService, mockDialogRef } = createComponent(RequestType.movie);
    comp.denyReason = 'Not interested';
    await comp.deny();
    expect(mockRequestService.denyMovie).toHaveBeenCalledWith({ id: 42, reason: 'Not interested', is4K: false });
    expect(mockDialogRef.close).toHaveBeenCalledWith({ denied: true, reason: 'Not interested' });
  });

  it('should deny TV request', async () => {
    const { comp, mockRequestService } = createComponent(RequestType.tvShow);
    await comp.deny();
    expect(mockRequestService.denyChild).toHaveBeenCalledWith({ id: 42, reason: undefined });
  });

  it('should deny album request', async () => {
    const { comp, mockRequestService } = createComponent(RequestType.album);
    await comp.deny();
    expect(mockRequestService.denyAlbum).toHaveBeenCalledWith({ id: 42, reason: undefined });
  });

  it('should show error when deny fails', async () => {
    const { comp, mockRequestService, mockMessageService, mockDialogRef } = createComponent();
    mockRequestService.denyMovie.mockReturnValue(of({ result: false }));
    await comp.deny();
    expect(mockMessageService.sendRequestEngineResultError).toHaveBeenCalled();
    expect(mockDialogRef.close).toHaveBeenCalledWith({ denied: false, reason: undefined });
  });

  it('should close dialog on onNoClick', () => {
    const { comp, mockDialogRef, data } = createComponent();
    comp.onNoClick();
    expect(mockDialogRef.close).toHaveBeenCalled();
    expect(data.denied).toBe(false);
  });
});
