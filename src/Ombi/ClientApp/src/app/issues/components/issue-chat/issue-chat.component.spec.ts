import { describe, it, expect, vi } from 'vitest';
import { IssueChatComponent } from './issue-chat.component';
import { of } from 'rxjs';

function createComponent() {
  const mockDialogRef = { close: vi.fn() };
  const data = { issueId: 42, title: 'Audio Issue' };
  const mockAuthService = {
    isAdmin: vi.fn().mockReturnValue(true),
    claims: vi.fn().mockReturnValue({ name: 'admin', roles: ['Admin'], email: 'admin@test.com' }),
  };
  const mockSettingsService = {
    getIssueSettings: vi.fn().mockReturnValue(of({ enabled: true })),
  };
  const mockIssueService = {
    getComments: vi.fn().mockReturnValue(of([
      { id: 1, comment: 'First comment', username: 'admin', date: new Date() },
      { id: 2, comment: 'Reply', username: 'user1', date: new Date() },
    ])),
    addComment: vi.fn().mockReturnValue(of({ comment: 'New comment', date: new Date(), user: { userName: 'admin' } })),
  };

  const comp = new IssueChatComponent(mockDialogRef as any, data, mockAuthService as any, mockSettingsService as any, mockIssueService as any);
  return { comp, mockDialogRef, mockIssueService };
}

describe('IssueChatComponent', () => {
  it('should load comments and map to messages on init', () => {
    const { comp, mockIssueService } = createComponent();
    comp.ngOnInit();
    expect(mockIssueService.getComments).toHaveBeenCalledWith(42);
    expect(comp.messages).toHaveLength(2);
    expect(comp.loaded).toBe(true);
    expect(comp.isAdmin).toBe(true);
  });

  it('should map sender/receiver based on username', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    // admin messages should be Sender (0), other users Receiver (1)
    expect(comp.messages[0].chatType).toBe(0); // Sender
    expect(comp.messages[1].chatType).toBe(1); // Receiver
  });

  it('should add comment and push to messages', () => {
    const { comp, mockIssueService } = createComponent();
    comp.ngOnInit();
    comp.addComment('Test reply');
    expect(mockIssueService.addComment).toHaveBeenCalledWith({ comment: 'Test reply', issueId: 42 });
    expect(comp.messages).toHaveLength(3);
  });

  it('should close dialog on close()', () => {
    const { comp, mockDialogRef } = createComponent();
    comp.close();
    expect(mockDialogRef.close).toHaveBeenCalled();
  });
});
