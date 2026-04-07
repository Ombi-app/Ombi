import { describe, it, expect, vi } from 'vitest';
import { RequestOptionsComponent } from './request-options.component';
import { RequestType } from '../../../interfaces';
import { of } from 'rxjs';

function createComponent(type: RequestType = RequestType.movie) {
  const data = { id: 42, type };

  const mockRequestService = {
    removeMovieRequestAsync: vi.fn().mockReturnValue(of({ result: true })),
    deleteChild: vi.fn().mockReturnValue(of({ result: true })),
    removeAlbumRequest: vi.fn().mockReturnValue(of({ result: true })),
    approveMovie: vi.fn().mockReturnValue(of({})),
    approveChild: vi.fn().mockReturnValue(of({})),
    approveAlbum: vi.fn().mockReturnValue(of({})),
    markMovieAvailable: vi.fn().mockReturnValue(of({})),
    markAlbumAvailable: vi.fn().mockReturnValue(of({})),
  };

  const mockMessageService = {
    send: vi.fn(),
    sendRequestEngineResultError: vi.fn(),
  };

  const mockDialog = {
    open: vi.fn().mockReturnValue({ afterClosed: () => of({ denied: true }) }),
  };

  const mockBottomSheetRef = { dismiss: vi.fn() };

  const mockTranslate = {
    instant: vi.fn((key: string) => key),
  };

  const comp = new RequestOptionsComponent(
    data, mockRequestService as any, mockMessageService as any,
    mockDialog as any, mockBottomSheetRef as any, mockTranslate as any,
  );

  return { comp, data, mockRequestService, mockMessageService, mockBottomSheetRef, mockDialog };
}

describe('RequestOptionsComponent', () => {
  describe('delete', () => {
    it('should delete movie request', () => {
      const { comp, mockRequestService, mockBottomSheetRef } = createComponent(RequestType.movie);
      comp.delete();
      expect(mockRequestService.removeMovieRequestAsync).toHaveBeenCalledWith(42);
      expect(mockBottomSheetRef.dismiss).toHaveBeenCalled();
    });

    it('should delete TV request', () => {
      const { comp, mockRequestService } = createComponent(RequestType.tvShow);
      comp.delete();
      expect(mockRequestService.deleteChild).toHaveBeenCalledWith(42);
    });

    it('should delete album request', () => {
      const { comp, mockRequestService } = createComponent(RequestType.album);
      comp.delete();
      expect(mockRequestService.removeAlbumRequest).toHaveBeenCalledWith(42);
    });

    it('should show error when delete fails', () => {
      const { comp, mockRequestService, mockMessageService } = createComponent(RequestType.movie);
      mockRequestService.removeMovieRequestAsync.mockReturnValue(of({ result: false, errorMessage: 'Fail' }));
      comp.delete();
      expect(mockMessageService.sendRequestEngineResultError).toHaveBeenCalled();
    });
  });

  describe('approve', () => {
    it('should approve movie request', async () => {
      const { comp, mockRequestService, mockBottomSheetRef } = createComponent(RequestType.movie);
      await comp.approve();
      expect(mockRequestService.approveMovie).toHaveBeenCalledWith({ id: 42, is4K: false });
      expect(mockBottomSheetRef.dismiss).toHaveBeenCalled();
    });

    it('should approve TV request', async () => {
      const { comp, mockRequestService } = createComponent(RequestType.tvShow);
      await comp.approve();
      expect(mockRequestService.approveChild).toHaveBeenCalledWith({ id: 42 });
    });

    it('should approve album request', async () => {
      const { comp, mockRequestService } = createComponent(RequestType.album);
      await comp.approve();
      expect(mockRequestService.approveAlbum).toHaveBeenCalledWith({ id: 42 });
    });
  });

  describe('approve4K', () => {
    it('should approve 4K movie request', async () => {
      const { comp, mockRequestService } = createComponent(RequestType.movie);
      await comp.approve4K();
      expect(mockRequestService.approveMovie).toHaveBeenCalledWith({ id: 42, is4K: true });
    });

    it('should not approve 4K for non-movie', async () => {
      const { comp, mockRequestService } = createComponent(RequestType.tvShow);
      await comp.approve4K();
      expect(mockRequestService.approveMovie).not.toHaveBeenCalled();
    });
  });

  describe('deny', () => {
    it('should open deny dialog and dismiss on denial', () => {
      const { comp, mockDialog, mockBottomSheetRef } = createComponent(RequestType.movie);
      comp.deny();
      expect(mockDialog.open).toHaveBeenCalled();
      expect(mockBottomSheetRef.dismiss).toHaveBeenCalled();
    });
  });

  describe('changeAvailability', () => {
    it('should mark movie as available', async () => {
      const { comp, mockRequestService } = createComponent(RequestType.movie);
      await comp.changeAvailability();
      expect(mockRequestService.markMovieAvailable).toHaveBeenCalledWith({ id: 42, is4K: false });
    });

    it('should mark album as available', async () => {
      const { comp, mockRequestService } = createComponent(RequestType.album);
      await comp.changeAvailability();
      expect(mockRequestService.markAlbumAvailable).toHaveBeenCalledWith({ id: 42 });
    });
  });
});
