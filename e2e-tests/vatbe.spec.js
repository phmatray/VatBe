const { test, expect } = require('@playwright/test');

const BASE_URL = 'https://vatbe.garry-ai.cloud';

test.describe('VatBE E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(BASE_URL);
    // Wait for the page to be fully loaded
    await page.waitForLoadState('networkidle');
  });

  test('Page loads successfully', async ({ page }) => {
    await expect(page).toHaveTitle(/VatBe/);
    await expect(page.locator('h1')).toContainText('VatBe');
  });

  test('Belgian mode: Valid KBO number 0477472701', async ({ page }) => {
    // Ensure Belgian mode is selected
    await page.click('button:has-text("🇧🇪 Belgian")');
    
    // Enter the number
    await page.fill('input[type="text"]', '0477472701');
    
    // Click Vérifier button
    await page.click('button:has-text("Vérifier")');
    
    // Wait for result to appear
    await page.waitForSelector('.result-card', { timeout: 15000 });
    
    // Check for success indicator
    const resultCard = page.locator('.result-card');
    const isValid = await resultCard.locator('.result-icon').textContent();
    
    console.log('Result icon:', isValid);
    
    // Take screenshot
    await page.screenshot({ path: 'screenshots/belgian-valid.png', fullPage: true });
    
    // The result should show valid format or VIES confirmation
    await expect(resultCard).toBeVisible();
  });

  test('Belgian mode: Valid KBO number with BE prefix', async ({ page }) => {
    await page.click('button:has-text("🇧🇪 Belgian")');
    await page.fill('input[type="text"]', 'BE0477472701');
    await page.click('button:has-text("Vérifier")');
    
    await page.waitForSelector('.result-card', { timeout: 15000 });
    await page.screenshot({ path: 'screenshots/belgian-with-prefix.png', fullPage: true });
    
    const resultCard = page.locator('.result-card');
    await expect(resultCard).toBeVisible();
  });

  test('Belgian mode: Invalid format', async ({ page }) => {
    await page.click('button:has-text("🇧🇪 Belgian")');
    await page.fill('input[type="text"]', '1234567890');
    await page.click('button:has-text("Vérifier")');
    
    // Should show inline error
    await page.waitForSelector('.inline-error', { timeout: 5000 });
    await page.screenshot({ path: 'screenshots/belgian-invalid.png', fullPage: true });
    
    const error = page.locator('.inline-error');
    await expect(error).toContainText('Format invalide');
  });

  test('EU mode: German VAT number', async ({ page }) => {
    // Switch to EU mode
    await page.click('button:has-text("🇪🇺 EU VAT")');
    
    // Select Germany
    await page.selectOption('select.country-select', 'DE');
    
    // Enter a test number
    await page.fill('input[type="text"]', '123456789');
    await page.click('button:has-text("Vérifier")');
    
    // Wait for result
    await page.waitForSelector('.result-card', { timeout: 15000 });
    await page.screenshot({ path: 'screenshots/eu-germany.png', fullPage: true });
    
    const resultCard = page.locator('.result-card');
    await expect(resultCard).toBeVisible();
  });

  test('EU mode: French VAT number', async ({ page }) => {
    await page.click('button:has-text("🇪🇺 EU VAT")');
    await page.selectOption('select.country-select', 'FR');
    await page.fill('input[type="text"]', '12345678901');
    await page.click('button:has-text("Vérifier")');
    
    await page.waitForSelector('.result-card', { timeout: 15000 });
    await page.screenshot({ path: 'screenshots/eu-france.png', fullPage: true });
  });

  test('Example buttons work - Belgian mode', async ({ page }) => {
    await page.click('button:has-text("🇧🇪 Belgian")');
    
    // Click first example button
    await page.click('.example-btn >> nth=0');
    
    // Should auto-trigger validation
    await page.waitForSelector('.result-card', { timeout: 15000 });
    await page.screenshot({ path: 'screenshots/example-button.png', fullPage: true });
    
    const resultCard = page.locator('.result-card');
    await expect(resultCard).toBeVisible();
  });

  test('Mode switching clears previous results', async ({ page }) => {
    // Start in Belgian mode, enter number
    await page.click('button:has-text("🇧🇪 Belgian")');
    await page.fill('input[type="text"]', '0477472701');
    await page.click('button:has-text("Vérifier")');
    await page.waitForSelector('.result-card', { timeout: 15000 });
    
    // Switch to EU mode
    await page.click('button:has-text("🇪🇺 EU VAT")');
    
    // Result should be cleared
    const resultCard = page.locator('.result-card');
    await expect(resultCard).not.toBeVisible();
    
    // Input should be cleared
    const input = page.locator('input[type="text"]');
    await expect(input).toHaveValue('');
    
    await page.screenshot({ path: 'screenshots/mode-switch.png', fullPage: true });
  });

  test('Console errors check', async ({ page }) => {
    const consoleErrors = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    await page.click('button:has-text("🇧🇪 Belgian")');
    await page.fill('input[type="text"]', '0477472701');
    await page.click('button:has-text("Vérifier")');
    await page.waitForSelector('.result-card', { timeout: 15000 });
    
    console.log('Console errors:', consoleErrors);
    
    if (consoleErrors.length > 0) {
      console.error('Found console errors:', consoleErrors);
    }
  });

  test('Network errors check', async ({ page }) => {
    const failedRequests = [];
    page.on('requestfailed', request => {
      failedRequests.push({
        url: request.url(),
        failure: request.failure()
      });
    });

    await page.click('button:has-text("🇧🇪 Belgian")');
    await page.fill('input[type="text"]', '0477472701');
    await page.click('button:has-text("Vérifier")');
    await page.waitForSelector('.result-card', { timeout: 15000 });
    
    console.log('Failed requests:', failedRequests);
    
    if (failedRequests.length > 0) {
      console.error('Found failed requests:', failedRequests);
    }
  });

  test('VIES API response check', async ({ page }) => {
    const apiResponses = [];
    
    page.on('response', async response => {
      const url = response.url();
      if (url.includes('vies') || url.includes('vat')) {
        apiResponses.push({
          url,
          status: response.status(),
          statusText: response.statusText()
        });
      }
    });

    await page.click('button:has-text("🇧🇪 Belgian")');
    await page.fill('input[type="text"]', '0477472701');
    await page.click('button:has-text("Vérifier")');
    await page.waitForSelector('.result-card', { timeout: 15000 });
    
    console.log('API responses:', JSON.stringify(apiResponses, null, 2));
  });
});
