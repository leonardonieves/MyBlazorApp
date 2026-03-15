// Wait for Stripe to be available
window.waitForStripe = async function(maxAttempts = 30, delayMs = 100) {
    console.log('Waiting for Stripe to load...');
    for (let i = 0; i < maxAttempts; i++) {
        if (typeof Stripe !== 'undefined') {
            console.log('✅ Stripe loaded successfully');
            return true;
        }
        await new Promise(resolve => setTimeout(resolve, delayMs));
        if (i % 10 === 0) {
            console.log(`Attempting to load Stripe... attempt ${i + 1}/${maxAttempts}`);
        }
    }
    console.error('❌ Stripe failed to load after ' + maxAttempts + ' attempts');
    return false;
};

// Check if Stripe is loaded
window.checkStripeExists = function() {
    const stripeExists = typeof Stripe !== 'undefined';
    console.log('Stripe exists:', stripeExists);
    return stripeExists;
};

// Function to redirect to Stripe Checkout using modern Stripe.js
window.redirectToCheckout = async function(sessionId, publishableKey) {
    try {
        console.log('redirectToCheckout called with sessionId:', sessionId);
        console.log('redirectToCheckout called with publishableKey:', publishableKey ? publishableKey.substring(0, 20) + '...' : 'NOT PROVIDED');

        // Validate inputs
        if (!sessionId) {
            throw new Error('Session ID is required');
        }
        if (!publishableKey) {
            throw new Error('Publishable Key is required');
        }

        // Wait for Stripe to be available
        console.log('Checking if Stripe is available...');
        const stripeLoaded = await window.waitForStripe();

        if (!stripeLoaded) {
            throw new Error('Stripe library failed to load. Please check your internet connection and reload the page.');
        }

        // Double-check Stripe is defined
        if (typeof Stripe === 'undefined') {
            console.error('Stripe is not defined after wait.');
            throw new Error('Stripe library is not available. Please reload the page.');
        }

        // Initialize Stripe with the public key
        console.log('Initializing Stripe...');
        let stripe;
        try {
            stripe = Stripe(publishableKey);
        } catch (stripeInitError) {
            console.error('Failed to initialize Stripe:', stripeInitError);
            throw new Error('Failed to initialize Stripe: ' + stripeInitError.message);
        }

        if (!stripe) {
            console.error('Stripe instance is null');
            throw new Error('Failed to create Stripe instance');
        }

        console.log('Stripe initialized successfully');

        // Use modern redirect method
        console.log('Redirecting to checkout with session:', sessionId);

        // Use redirectToCheckout (still supported but newer method preferred)
        try {
            const { error } = await stripe.redirectToCheckout({ sessionId: sessionId });

            if (error) {
                console.error('Stripe checkout error:', error);
                throw new Error('Stripe checkout error: ' + error.message);
            }

            console.log('Successfully redirected to Stripe Checkout');
        } catch (redirectError) {
            console.error('Redirect error:', redirectError);
            // If redirectToCheckout fails, try using the client-only session API
            console.log('Attempting alternative checkout method...');
            window.location.href = 'https://checkout.stripe.com/pay/' + sessionId;
        }
    } catch (err) {
        console.error('❌ Error in redirectToCheckout:', err);
        console.error('Stack:', err.stack);
        throw err;
    }
};
