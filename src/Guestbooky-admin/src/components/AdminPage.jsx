import useMessages from "../hooks/useMessages.js";

import './AdminPage.css'


const AdminPage = () => {
    const {messages, totalMessages, loading, error, removeMessage, page, nextPage, previousPage} = useMessages();
    const localeOptions = {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: 'numeric',
        minute: 'numeric',
        second: 'numeric',
        timeZoneName: 'short'
    }

    if (loading) {
        return (<div className='warning'><p>Loading...</p></div>);
    }

    if (messages.length === 0 && page > 1) {
        previousPage();
    }

    if (messages.length === 0) {
        return (<div className='warning'><p>No messages.</p></div>);
    }

    return (
        <>
            <div className='warning'><p>{error ? 'Error : ' + error : 'Total messages: ' + totalMessages }</p></div>
            <ul>
            {messages.length > 0 ? messages.map((message, index) => (
                    <li key={message.id} className='list-item' style={{ '--child-index': index}} >
                        <h2>{message.author}</h2>
                        <h3>{new Date(message.timestamp).toLocaleString(navigator.language, localeOptions)}</h3>
                        <button onClick={() => removeMessage(message.id)}>Delete</button>
                        <div className='message-text'><p>{message.message}</p></div>
                    </li>
                )) : ''}
            </ul>
            <div className='navigation'>
                <button onClick={() => previousPage()}>Previous Page</button>
                <button onClick={() => nextPage()}>Next Page</button>
            </div>
        </>
    );
};
export default AdminPage;