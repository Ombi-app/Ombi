import { describe, it, expect, vi, beforeEach } from 'vitest';
import { RoleDirective } from './role-directive';

describe('RoleDirective', () => {
  let mockTemplateRef: any;
  let mockViewContainer: any;
  let mockAuth: any;

  beforeEach(() => {
    mockTemplateRef = {};
    mockViewContainer = {
      createEmbeddedView: vi.fn(),
      clear: vi.fn(),
    };
    mockAuth = {
      hasRole: vi.fn().mockReturnValue(false),
    };
  });

  function createDirective() {
    return new RoleDirective(mockTemplateRef, mockViewContainer, mockAuth);
  }

  it('should show the element when user has the required role', () => {
    mockAuth.hasRole.mockImplementation((role: string) => role === 'RequestMovie');
    const directive = createDirective();
    directive.permission = 'RequestMovie';

    expect(mockViewContainer.createEmbeddedView).toHaveBeenCalledWith(mockTemplateRef);
  });

  it('should show the element when user is admin', () => {
    mockAuth.hasRole.mockImplementation((role: string) => role === 'admin');
    const directive = createDirective();
    directive.permission = 'RequestMovie';

    expect(mockViewContainer.createEmbeddedView).toHaveBeenCalledWith(mockTemplateRef);
  });

  it('should hide the element when user lacks the role and is not admin', () => {
    mockAuth.hasRole.mockReturnValue(false);
    const directive = createDirective();
    directive.permission = 'RequestMovie';

    expect(mockViewContainer.clear).toHaveBeenCalled();
    expect(mockViewContainer.createEmbeddedView).not.toHaveBeenCalled();
  });

  it('should not recreate embedded view if already shown', () => {
    mockAuth.hasRole.mockImplementation((role: string) => role === 'admin');
    const directive = createDirective();

    directive.permission = 'RequestMovie';
    directive.permission = 'RequestMovie';

    // createEmbeddedView should be called only once
    expect(mockViewContainer.createEmbeddedView).toHaveBeenCalledTimes(1);
  });

  it('should clear and then show when role changes from unauthorized to authorized', () => {
    mockAuth.hasRole.mockReturnValue(false);
    const directive = createDirective();
    directive.permission = 'RequestMovie';

    expect(mockViewContainer.clear).toHaveBeenCalledTimes(1);
    expect(mockViewContainer.createEmbeddedView).not.toHaveBeenCalled();

    // Now grant the role
    mockAuth.hasRole.mockImplementation((role: string) => role === 'RequestTv');
    directive.permission = 'RequestTv';

    expect(mockViewContainer.createEmbeddedView).toHaveBeenCalledTimes(1);
  });

  it('should not call updateView on ngOnInit if permission is not yet set', () => {
    const directive = createDirective();
    // roleName is undefined at this point, so hasRole(undefined) returns false
    directive.ngOnInit();

    // The view should be cleared (hidden) since no role is set
    expect(mockViewContainer.clear).toHaveBeenCalled();
  });
});
