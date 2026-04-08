import { describe, it, expect, vi } from 'vitest';
import { DetailsGroupComponent } from './details-group.component';
import { IssueStatus } from '../../../interfaces';
import { of } from 'rxjs';

function createComponent() {
  const mockTranslate = { instant: vi.fn((key: string) => key) };
  const mockIssuesService = {
    deleteIssue: vi.fn().mockResolvedValue(true),
    updateStatus: vi.fn().mockReturnValue(of(true)),
  };
  const mockNotify = { success: vi.fn() };
  const mockDialog = { open: vi.fn() };

  const comp = new DetailsGroupComponent(mockTranslate as any, mockIssuesService as any, mockNotify as any, mockDialog as any);
  comp.issue = { id: 1, requestId: 42, status: IssueStatus.Pending } as any;
  return { comp, mockIssuesService, mockNotify, mockDialog };
}

describe('DetailsGroupComponent', () => {
  it('should return hasRequest true when requestId exists', () => {
    const { comp } = createComponent();
    expect(comp.hasRequest).toBe(true);
  });

  it('should return hasRequest false when no requestId', () => {
    const { comp } = createComponent();
    comp.issue = { id: 1, requestId: null } as any;
    expect(comp.hasRequest).toBe(false);
  });

  it('should delete issue and mark as deleted', async () => {
    const { comp, mockIssuesService, mockNotify } = createComponent();
    await comp.delete(comp.issue);
    expect(mockIssuesService.deleteIssue).toHaveBeenCalledWith(1);
    expect(comp.deleted).toBe(true);
    expect(mockNotify.success).toHaveBeenCalled();
  });

  it('should resolve issue', () => {
    const { comp, mockIssuesService } = createComponent();
    comp.resolve(comp.issue);
    expect(mockIssuesService.updateStatus).toHaveBeenCalledWith({ issueId: 1, status: IssueStatus.Resolved });
  });

  it('should set issue in progress', () => {
    const { comp, mockIssuesService } = createComponent();
    comp.inProgress(comp.issue);
    expect(mockIssuesService.updateStatus).toHaveBeenCalledWith({ issueId: 1, status: IssueStatus.InProgress });
  });

  it('should open chat dialog', () => {
    const { comp, mockDialog } = createComponent();
    comp.openChat(comp.issue);
    expect(mockDialog.open).toHaveBeenCalled();
  });
});
