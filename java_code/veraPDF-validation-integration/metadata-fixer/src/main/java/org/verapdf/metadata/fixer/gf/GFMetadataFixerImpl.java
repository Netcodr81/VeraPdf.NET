/*
 * This file is part of veraPDF Metadata Fixer, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Metadata Fixer is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Metadata Fixer as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Metadata Fixer as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.metadata.fixer.gf;

import org.verapdf.metadata.fixer.gf.impl.model.PDFDocumentImpl;
import org.verapdf.metadata.fixer.utils.parser.XMLProcessedObjectsParser;
import org.verapdf.pdfa.PDFAParser;
import org.verapdf.pdfa.results.MetadataFixerResult;
import org.verapdf.pdfa.results.ValidationResult;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

/**
 * Connects the generic metadata repair algorithm to GreenField documents.
 *
 * <p>One overload opens a document from a stream; the other reuses the
 * document provided by a PDF/A parser. Both delegate the repair, its
 * validation result and the processed-objects parser to the base class.</p>
 *
 * <p><b>.NET port:</b> Map input and output to {@code System.IO.Stream},
 * and port the fixer and result contracts with their document-lifetime
 * semantics. The current {@code close()} method does nothing; do not assume
 * it disposes either stream when designing an {@code IDisposable} API.</p>
 *
 * @author Maksim Bezrukov
 */
public final class GFMetadataFixerImpl extends MetadataFixerImpl {

	public GFMetadataFixerImpl() {

	}

	@Override
	public MetadataFixerResult fixMetadata(InputStream toFix, OutputStream outputRepaired, ValidationResult result)
			throws IOException {
		return fixMetadata(outputRepaired, new PDFDocumentImpl(toFix), result, true,
				XMLProcessedObjectsParser.getInstance());
	}

	@Override
	public MetadataFixerResult fixMetadata(PDFAParser parser, OutputStream outputRepaired, ValidationResult result) {
		return fixMetadata(outputRepaired, parser.getPDFDocument(), result, true,
				XMLProcessedObjectsParser.getInstance());
	}

	@Override
	public void close() {
		// TODO Auto-generated method stub

	}

}
