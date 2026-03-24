let razorpay = null;

function initializeRazorpay(options) {
    try {
        razorpay = new Razorpay(options);

        razorpay.on('payment.success', function(response) {
            options.handler.success(
                response.razorpay_payment_id,
                response.razorpay_order_id,
                response.razorpay_signature
            );
        });

        razorpay.on('payment.failed', function(response) {
            options.handler.failure(
                response.error.code,
                response.error.description
            );
        });

        razorpay.open();
    } catch (error) {
        console.error('Razorpay initialization failed:', error);
        options.handler.failure('INIT_FAILED', error.message);
    }
}

// Cleanup
window.addEventListener('beforeunload', function() {
    if (razorpay) {
        razorpay.close();
        razorpay = null;
    }
});