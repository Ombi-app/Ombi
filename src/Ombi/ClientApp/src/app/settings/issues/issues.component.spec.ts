import { describe, it, expect, vi } from 'vitest';
import { IssuesComponent } from './issues.component';
import { UntypedFormBuilder } from '@angular/forms';
import { of } from 'rxjs';

function createComponent() {
  const mockIssuesService = {
    getCategories: vi.fn().mockReturnValue(of([{ id: 1, value: 'Audio' }, { id: 2, value: 'Video' }])),
    createCategory: vi.fn().mockReturnValue(of(true)),
    deleteCategory: vi.fn().mockReturnValue(of(true)),
  };
  const mockSettingsService = {
    getIssueSettings: vi.fn().mockReturnValue(of({
      enabled: true, enableInProgress: false, deleteIssues: false, daysAfterResolvedToDelete: 7,
    })),
    saveIssueSettings: vi.fn().mockReturnValue(of(true)),
  };
  const fb = new UntypedFormBuilder();
  const mockNotify = { error: vi.fn(), success: vi.fn() };

  const comp = new IssuesComponent(mockIssuesService as any, mockSettingsService as any, fb, mockNotify as any);
  return { comp, mockIssuesService, mockSettingsService, mockNotify };
}

describe('IssuesComponent', () => {
  it('should load settings and categories on init', () => {
    const { comp, mockIssuesService, mockSettingsService } = createComponent();
    comp.ngOnInit();
    expect(comp.form).toBeDefined();
    expect(comp.categories).toHaveLength(2);
    expect(mockIssuesService.getCategories).toHaveBeenCalled();
    expect(mockSettingsService.getIssueSettings).toHaveBeenCalled();
  });

  it('should add category and refresh list', () => {
    const { comp, mockIssuesService } = createComponent();
    comp.ngOnInit();
    comp.categoryToAdd = { id: 0, value: 'Subtitle' };
    comp.addCategory();
    expect(mockIssuesService.createCategory).toHaveBeenCalled();
    // After successful add, value is reset and categories are refreshed
    expect(comp.categoryToAdd.value).toBe('');
    // getCategories called at least twice: init + addCategory refresh
    expect(mockIssuesService.getCategories.mock.calls.length).toBeGreaterThanOrEqual(2);
  });

  it('should delete category and refresh list', () => {
    const { comp, mockIssuesService } = createComponent();
    comp.ngOnInit();
    comp.deleteCategory(1);
    expect(mockIssuesService.deleteCategory).toHaveBeenCalledWith(1);
    expect(mockIssuesService.getCategories.mock.calls.length).toBeGreaterThanOrEqual(2);
  });

  it('should save and notify success', () => {
    const { comp, mockNotify, mockSettingsService } = createComponent();
    comp.ngOnInit();
    comp.onSubmit(comp.form);
    expect(mockSettingsService.saveIssueSettings).toHaveBeenCalled();
    expect(mockNotify.success).toHaveBeenCalledWith('Successfully saved the Issue settings');
  });

  it('should error when deleteIssues enabled but days <= 0', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.form.controls['deleteIssues'].setValue(true);
    comp.form.controls['daysAfterResolvedToDelete'].setValue(0);
    comp.onSubmit(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('You need to enter days greater than 0');
  });

  it('should error when deleteIssues enabled and days is negative', () => {
    const { comp, mockNotify } = createComponent();
    comp.ngOnInit();
    comp.form.controls['deleteIssues'].setValue(true);
    comp.form.controls['daysAfterResolvedToDelete'].setValue(-1);
    comp.onSubmit(comp.form);
    expect(mockNotify.error).toHaveBeenCalledWith('You need to enter days greater than 0');
  });
});
