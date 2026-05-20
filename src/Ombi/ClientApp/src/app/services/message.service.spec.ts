import { describe, it, expect, beforeEach, vi } from 'vitest';
import { MessageService } from './message.service';

function createMockMessageService() {
  const mockSnackBar = {
    open: vi.fn(),
  };
  const mockTranslate = {
    instant: vi.fn(),
  };

  const service = Object.create(MessageService.prototype);
  service.snackBar = mockSnackBar;
  service.translate = mockTranslate;
  service.config = { duration: 4000 };

  return { service: service as MessageService, mockSnackBar, mockTranslate };
}

describe('MessageService', () => {
  let service: MessageService;
  let mockSnackBar: ReturnType<typeof createMockMessageService>['mockSnackBar'];
  let mockTranslate: ReturnType<typeof createMockMessageService>['mockTranslate'];

  beforeEach(() => {
    const mocks = createMockMessageService();
    service = mocks.service;
    mockSnackBar = mocks.mockSnackBar;
    mockTranslate = mocks.mockTranslate;
  });

  describe('send', () => {
    it('should open snackbar with default OK action', () => {
      service.send('Hello');
      expect(mockSnackBar.open).toHaveBeenCalledWith('Hello', 'OK', { duration: 4000 });
    });

    it('should open snackbar with custom action uppercased', () => {
      service.send('Hello', 'dismiss');
      expect(mockSnackBar.open).toHaveBeenCalledWith('Hello', 'DISMISS', { duration: 4000 });
    });
  });

  describe('sendRequestEngineResultError', () => {
    it('should use translated error code when translation exists', () => {
      mockTranslate.instant.mockReturnValue('Translated error message');
      service.sendRequestEngineResultError({ errorCode: 0, result: false, message: '', errorMessage: '', requestId: undefined });
      expect(mockSnackBar.open).toHaveBeenCalledWith('Translated error message', 'OK', { duration: 4000 });
    });

    it('should fall back to errorMessage when translation key matches input', () => {
      const errorCode = 0;
      const textKey = 'Requests.ErrorCodes.' + errorCode;
      mockTranslate.instant.mockReturnValue(textKey); // translation not found
      service.sendRequestEngineResultError({
        errorCode,
        result: false,
        message: 'fallback message',
        errorMessage: 'error msg',
        requestId: undefined,
      });
      expect(mockSnackBar.open).toHaveBeenCalledWith('error msg', 'OK', { duration: 4000 });
    });

    it('should fall back to message when errorMessage is empty', () => {
      const errorCode = 0;
      const textKey = 'Requests.ErrorCodes.' + errorCode;
      mockTranslate.instant.mockReturnValue(textKey);
      service.sendRequestEngineResultError({
        errorCode,
        result: false,
        message: 'fallback message',
        errorMessage: '',
        requestId: undefined,
      });
      expect(mockSnackBar.open).toHaveBeenCalledWith('fallback message', 'OK', { duration: 4000 });
    });

    it('should fall back to generic error when no messages available', () => {
      const errorCode = 0;
      const textKey = 'Requests.ErrorCodes.' + errorCode;
      mockTranslate.instant.mockImplementation((key: string) => {
        if (key === textKey) return textKey;
        if (key === 'ErrorPages.SomethingWentWrong') return 'Something went wrong';
        return key;
      });
      service.sendRequestEngineResultError({
        errorCode,
        result: false,
        message: '',
        errorMessage: '',
        requestId: undefined,
      });
      expect(mockSnackBar.open).toHaveBeenCalledWith('Something went wrong', 'OK', { duration: 4000 });
    });

    it('should use custom action parameter', () => {
      mockTranslate.instant.mockReturnValue('Error');
      service.sendRequestEngineResultError(
        { errorCode: 0, result: false, message: '', errorMessage: '', requestId: undefined },
        'Close'
      );
      expect(mockSnackBar.open).toHaveBeenCalledWith('Error', 'CLOSE', { duration: 4000 });
    });
  });
});
