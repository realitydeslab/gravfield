import { currentTime } from '@most/scheduler';
import { sample, periodic } from '@coda/core';

// const performers = [
//   {
//     id:"A",
//     pos:[0,0,0]
//   },
//   {
//     id:"B",
//     pos:[0,0,0]
//   },
//   {
//     id:"C",
//     pos:[0,0,0]
//   }
// ];

const performers = {
  A:{},
  B:{},
  C:{}
}

/**
 * Try to propagate an event or propagate an error to the stream
 * @ignore
 */
function tryEvent(t, x, sink) {
  try {
    sink.event(t, x);
  } catch (e) {
    sink.error(t, e);
  }
}

/**
 * @ignore
 */
function createStream(id, channel, size) {
  return {
    attr: {
      format: 'vector',
      size,
      samplerate: 100,
    },
    run(sink, scheduler) {
      performers[id][channel] = (m) => {
        if (Object.keys(m).includes(channel)) {
          tryEvent(currentTime(scheduler), m[channel], sink);
        }
      };
      return {
        dispose() {
          delete performers[id][channel];
        },
      };
    },
  };
}

/*

function CheckPerformerId(id)
{
  if(typeof(id) === 'number')
    return id == 1 ? "B" : (id == 2 ? "C" : "A");

  if(typeof(id) === 'string')
    return id == "b" ? "B" : (id == "c" ? "C" : "A");
  
  return id;
}

const host = 'ws://localhost:19090';
const socket = new WebSocket(host);
socket.onerror = () => {
  // eslint-disable-next-line no-console
  console.error('[unity] Error: the websocket server is unavailable at', host);
};
socket.onclose = () => {
  // eslint-disable-next-line no-console
  console.error('[unity] Error: lost connection with the websocket server');
};
socket.onmessage = (json) => {
  const m = JSON.parse(json.data);
  
  Object.values(performers[m.id]).forEach((f) => {
    f(m);
  });
};



export const performer = function(id = 0) {

  id = CheckPerformerId(id);
  return {
    pos:sample(createStream(id, 'pos', 3), periodic(10))
  }
}

export const sendosc = function(address, source)
{
  message = {
    type:"osc",
    address:address,
    value:source.value,
  };
console.log(address);
console.log(source.value);
console.log(source.values);
console.log(source.value.construtor === Array);

  socket.send(JSON.stringify(message));
  return source;
}

*/