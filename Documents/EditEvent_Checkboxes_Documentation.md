# EditEvent View - Checkbox Documentation

This document describes the purpose and functionality of each checkbox in the EditEvent view.

## Event Details Section

### 1. Show on application Calendar?
- **Field Name**: `ShowOnCalender`
- **Purpose**: Controls whether this event appears in the mobile application's calendar view
- **When Checked**: The event will be visible in the app's calendar interface for users to see
- **When Unchecked**: The event will not appear in the calendar view (hidden from calendar)

### 2. Send Invitation?
- **Field Name**: `SendInvitation`
- **Purpose**: Controls whether invitation messages should be sent to guests
- **When Checked**: The system will send invitation messages to all guests for this event
- **When Unchecked**: No invitation messages will be sent

### 3. Whatsapp Confirmation?
- **Field Name**: `WhatsappConfirmation`
- **Purpose**: Enables WhatsApp-based attendance confirmation requests
- **When Checked**: Guests will receive WhatsApp messages asking them to confirm their attendance
- **When Unchecked**: No WhatsApp confirmation requests will be sent

### 4. Whatsapp Push?
- **Field Name**: `WhatsappPush`
- **Purpose**: Enables push notifications via WhatsApp for this event
- **When Checked**: WhatsApp push notifications will be sent to guests
- **When Unchecked**: No WhatsApp push notifications will be sent

## Failed Sending Details Section

### 5. Show Event Location Link?
- **Field Name**: `ShowFailedSendingEventLocationLink`
- **Purpose**: Controls whether the event location link is shown in failed sending recovery messages
- **When Checked**: If a message fails to send, the retry/recovery message will include the event location link
- **When Unchecked**: Failed sending recovery messages will not include the location link

### 6. Show congratulation Link?
- **Field Name**: `ShowFailedSendingCongratulationLink`
- **Purpose**: Controls whether the congratulation/thanks link is shown in failed sending recovery messages
- **When Checked**: If a message fails to send, the retry/recovery message will include the congratulation link
- **When Unchecked**: Failed sending recovery messages will not include the congratulation link

## Summary

These checkboxes provide fine-grained control over:
- **Event visibility** in the mobile app calendar
- **Message sending** (invitations, confirmations, push notifications)
- **Failed message recovery** options (which links to include in retry attempts)

All checkboxes default to their stored database values when editing an existing event, or can be set during event creation.
