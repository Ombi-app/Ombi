
import { navBar as NavBar } from './shared/NavBar';

export abstract class BasePage {
  // Abstract visit methods
  abstract visit(options?: any): Cypress.Chainable<any>;
  abstract visit(id?: string, options?: any): Cypress.Chainable<any>;

  // Common page properties
  navbar = NavBar;

  // Common page methods
  /**
   * Wait for page to be fully loaded
   */
  waitForPageLoad(): Cypress.Chainable<any> {
    return cy.get('body').should('be.visible');
  }

  /**
   * Check if page is loaded by verifying a unique element
   */
  isPageLoaded(uniqueSelector: string): Cypress.Chainable<any> {
    return cy.get(uniqueSelector).should('exist').then(() => true);
  }

  /**
   * Scroll to element with smooth behavior
   */
  scrollToElement(selector: string): Cypress.Chainable<any> {
    return cy.get(selector).scrollIntoView();
  }

  /**
   * Wait for loading spinner to disappear
   */
  waitForLoadingToComplete(): Cypress.Chainable<any> {
    return cy.get('[data-test="loading-spinner"]', { timeout: 10000 }).should('not.exist');
  }

  /**
   * Get element by data-test attribute
   */
  getByData(selector: string): Cypress.Chainable<any> {
    return cy.get(`[data-test="${selector}"]`);
  }

  /**
   * Get element by data-test attribute with partial match
   */
  getByDataLike(selector: string): Cypress.Chainable<any> {
    return cy.get(`[data-test*="${selector}"]`);
  }

  /**
   * Check if element is visible and enabled
   */
  isElementInteractive(selector: string): Cypress.Chainable<any> {
    return cy.get(selector)
      .should('be.visible')
      .should('not.be.disabled')
      .then(() => true);
  }

  /**
   * Take screenshot of current page
   */
  takeScreenshot(name?: string): Cypress.Chainable<any> {
    const screenshotName = name || `${this.constructor.name}_${Date.now()}`;
    return cy.screenshot(screenshotName);
  }

  /**
   * Log page action for debugging
   */
  logAction(action: string, details?: any): void {
    // Use console.log instead of cy.log to avoid promise/cy command mixing
    console.log(`[${this.constructor.name}] ${action}`, details);
  }
}
