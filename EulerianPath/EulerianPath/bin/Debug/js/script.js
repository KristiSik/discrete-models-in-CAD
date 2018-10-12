var gId = 1;
var selectRootNodeMode = false;
var firstClickAfterDFS = false;
var DFSDisplayMode = false;
var rootNode;
var cy = cytoscape({
    container: document.getElementById('cy'),
    zoom: 1.5,
    minZoom: 0.5,
    maxZoom: 10,
    wheelSensitivity: 0.1,
    style: [
        {
        selector: 'node',
        style: {
            'background-color': '#666',
            'label': 'data(id)'
        },
        },
        {
        selector: '.selected-node',
        style: {
            'background-color': 'red'
        },
        },
        {
        selector: 'edge',
        style: {
            'width': 3,
            'label': 'data(label)',
            'font-weight': 'bold',
            'text-margin-x' : 15,
            'text-margin-y': -15,
            'line-color': '#ccc',
            'target-arrow-color': '#ccc',
            'curve-style': 'bezier',
            'target-arrow-shape': 'triangle',
            'arrow-scale' : 1.5
        }
        },
        {
        selector: '.visited-node',
        style: {
            'background-color': '#ffca00'
        }
        },
        {
        selector: '.selected-edge-red',
        style: {
            'width': 4,
            'line-color': '#f44242',
            'transition-property': 'line-color',
            'transition-duration': '0.5s'
        }
        },
        {
        selector: '.root-node',
        style: {
            'background-color': 'orange'
        }
        }
    ]
});

var data;

$.get("result.json", function (result) {
    data = result;
    data.Nodes.forEach(function(node) {
        cy.add({
            group: "nodes",
            data: { id: node.Name }
        });
    });
    data.Edges.forEach(function (edge) {
        cy.add({
            group: "edges",
            data: { source: edge.StartNode.Name, target: edge.EndNode.Name, label: edge.Weight }
        });
    });

    cy.layout({
        name: "circle"
    }).run();
});