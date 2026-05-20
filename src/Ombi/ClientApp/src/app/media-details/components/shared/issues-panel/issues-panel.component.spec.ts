import { describe, it, expect, vi } from 'vitest';
import { IssuesPanelComponent } from './issues-panel.component';
import { IssueStatus } from '../../../../interfaces';
import { of } from 'rxjs';

function createComponent() {
  const mockIssuesService = {
    getIssuesByProviderId: vi.fn().mockResolvedValue([
      { id: 1, status: IssueStatus.Pending, subject: 'Audio' },
      { id: 2, status: IssueStatus.Resolved, subject: 'Video' },
    ]),
    updateStatus: vi.fn().mockReturnValue(of(true)),
    deleteIssue: vi.fn().mockResolvedValue(true),
  };
  const mockNotify = { success: vi.fn(), error: vi.fn() };
  const mockTranslate = { instant: vi.fn((key: string) => key) };
  const mockSettingsService = {
    getIssueSettings: vi.fn().mockReturnValue(of({ enabled: true })),
  };

  const comp = new IssuesPanelComponent(mockIssuesService as any, mockNotify as any, mockTranslate as any, mockSettingsService as any);
  comp.providerId = 'tmdb-550';
  return { comp, mockIssuesService, mockNotify };
}

describe('IssuesPanelComponent', () => {
  it('should load issues on init', async () => {
    const { comp, mockIssuesService } = createComponent();
    await comp.ngOnInit();
    expect(mockIssuesService.getIssuesByProviderId).toHaveBeenCalledWith('tmdb-550');
    expect(comp.issues).toHaveLength(2);
    expect(comp.issuesCount).toBe(2);
    expect(comp.isOutstanding).toBe(true);
  });

  it('should resolve an issue', async () => {
    const { comp, mockIssuesService, mockNotify } = createComponent();
    await comp.ngOnInit();
    comp.resolve(comp.issues[0]);
    expect(mockIssuesService.updateStatus).toHaveBeenCalledWith({ issueId: 1, status: IssueStatus.Resolved });
  });

  it('should set issue in progress', async () => {
    const { comp, mockIssuesService } = createComponent();
    await comp.ngOnInit();
    comp.inProgress(comp.issues[0]);
    expect(mockIssuesService.updateStatus).toHaveBeenCalledWith({ issueId: 1, status: IssueStatus.InProgress });
  });

  it('should delete an issue and update count', async () => {
    const { comp, mockIssuesService } = createComponent();
    await comp.ngOnInit();
    await comp.delete(comp.issues[0]);
    expect(mockIssuesService.deleteIssue).toHaveBeenCalledWith(1);
    expect(comp.issues).toHaveLength(1);
    expect(comp.issuesCount).toBe(1);
  });
});
